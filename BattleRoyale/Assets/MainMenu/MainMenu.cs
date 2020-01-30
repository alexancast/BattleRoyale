using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI elapsedTimeText;

    private int elapsedSeconds;
    private int elapsedMinutes;
    private Coroutine searchCoroutine;

    public void StartSearch()
    {
        searchCoroutine = StartCoroutine(Searching());
    }

    public void StopSearch()
    {
        if(searchCoroutine != null)
        {
            StopCoroutine(searchCoroutine);
        }
    }

    public IEnumerator Searching() {

        elapsedMinutes = 0;
        elapsedSeconds = 0;
        elapsedTimeText.text = "Time elapsed: 00:00";

        while (true)
        {
            yield return new WaitForSeconds(1);
            elapsedSeconds++;
            if(elapsedSeconds == 60)
            {
                elapsedMinutes++;
                elapsedSeconds = 0;
            }

            elapsedTimeText.text = "Time elapsed: " + elapsedMinutes.ToString("00") + ":" + elapsedSeconds.ToString("00");
        }
    
    }
}
