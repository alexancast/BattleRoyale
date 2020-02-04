using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEvent : MonoBehaviour
{

    public static GameEvent instance;

    private GameObject player;

    public void Awake()
    {
        if (instance == null)
            instance = this;

        if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

    }


    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Setup();
        RunGame();
    }

    public virtual void RunGame()
    {
        player = Instantiate(NetClient.instance.GetCharacter().GetPrefab(), null);
    }

    public virtual void Setup()
    {
        NetClient.instance.SetGameEvent(this);
        Debug.Log("game event set");
    }

    public virtual void Spawn(GameObject objectToSpawn)
    {

    }

    public virtual void RegisterKill(int killedBy) {  }

    public virtual void LoadPlayer()
    {

        Player newPlayer = Instantiate(NetClient.instance.GetCharacter().GetPrefab(), null).GetComponent<Player>();
        NetClient.instance.SetPlayer(newPlayer);
        Destroy(player);
        player = newPlayer.gameObject;
        Spawn(newPlayer.gameObject);
     


    }

    

}
