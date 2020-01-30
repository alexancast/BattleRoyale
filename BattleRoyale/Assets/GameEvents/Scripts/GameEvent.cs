using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEvent : MonoBehaviour
{

    public static GameEvent instance;

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
        Instantiate(NetCient.instance.GetCharacter().GetPrefab(), new Vector3(0, 0, 0), Quaternion.identity, null);
    }

    public virtual void Setup()
    {
        NetCient.instance.SetGameEvent(this);
        Debug.Log("game event set");
    }

    public virtual void Spawn(GameObject objectToSpawn)
    {

    }

    public virtual void RegisterKill(int killedBy) {  }

    public virtual void LoadPlayer()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Instantiate(NetCient.instance.GetCharacter().GetPrefab(), new Vector3(0,0,0), Quaternion.identity, null);
     


    }

    

}
