using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MostKillsOnTime : GameEvent
{
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private TextMeshProUGUI gameFinished;
    [Tooltip("Time the game will last in seconds")]
    [SerializeField] private float gameTime;
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private GameObject scorePrefab;
    [SerializeField] private bool debugMode;
    [SerializeField] private float slowMotionDivider = 0.05f;

    private bool isRunning = false;
    private float timeRemaining = 20;
    private GameObject[] spawnPoints;
    private int[] kills;
    private GameObject[] scores;


    public override void RunGame()
    {
        base.RunGame();

        kills = new int[NetClient.instance.GetConnectedPeers().Length];
        scores = new GameObject[NetClient.instance.GetConnectedPeers().Length];

        for (int i = 0; i < kills.Length; i++)
        {
            scores[i] = Instantiate(scorePrefab, scoreboard.transform);
            scores[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Player: " + i;
        }

        Debug.Log("Game started");
        timeRemaining = gameTime;
        isRunning = true;
        timer.gameObject.SetActive(true);
    }

    public override void RegisterKill(int peerIndex)
    {
        base.RegisterKill(peerIndex);
        kills[peerIndex]++;
        scores[peerIndex].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "kills: " + kills[peerIndex];
    }

    public int FindWinner()
    {
        int winner = 0;

        for (int i = 0; i < kills.Length; i++)
        {
            if (kills[i] > kills[winner])
            {
                winner = i;
            }
        }
        return winner;
    }

    public override void Setup()
    {
        base.Setup();
    }

    public override void Spawn(GameObject objectToSpawn)
    {
        base.Spawn(objectToSpawn);
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        int value = Random.Range(0, spawnPoints.Length-1);
        objectToSpawn.transform.position = spawnPoints[value].transform.position;
    }

    public void Update()
    {
        if (isRunning && timer.gameObject.activeSelf)
        {
            timeRemaining -= Time.deltaTime;

            int minutesLeft = Mathf.FloorToInt(timeRemaining / 60);
            int secondsLeft = Mathf.RoundToInt(timeRemaining - minutesLeft * 60);
            timer.text = minutesLeft.ToString("00") + ":" + secondsLeft.ToString("00");
        }

        if(timeRemaining <= 0 && isRunning)
        {
            timer.gameObject.SetActive(false);
            isRunning = false;
            Time.timeScale = slowMotionDivider;
            gameFinished.gameObject.SetActive(true);

            if (NetClient.instance.GetPeerIndex() == FindWinner()) {

                gameFinished.text = "Victory";
                gameFinished.color = Color.green;
            }
            else
            {
                gameFinished.text = "Defeat";
                gameFinished.color = Color.red;

            }
            StartCoroutine(ReturnToMenu());
        }

        if (Input.GetKeyDown(KeyCode.Tab))
            scoreboard.transform.parent.gameObject.SetActive(true);
        else if (Input.GetKeyUp(KeyCode.Tab))
            scoreboard.transform.parent.gameObject.SetActive(false);
    }

    public IEnumerator ReturnToMenu()
    {
        float delayBeforeReturning = 5;

        while (delayBeforeReturning > 0)
        {
            yield return null;
            delayBeforeReturning -= Time.deltaTime * (1 / slowMotionDivider);
        }

        Time.timeScale = 1;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        NetClient.instance.DisconnectFromServer();
        gameFinished.gameObject.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }
}
