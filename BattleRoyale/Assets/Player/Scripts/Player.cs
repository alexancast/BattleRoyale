using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100;

    [Header("References")]
    [SerializeField] private Image healthFill;
    private Camera currentCamera;
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject youDiedText;
    [SerializeField] private RespawnUI respawnUI;
    [SerializeField] private Animation skullAnimation;
    [SerializeField] private Special special;
    [SerializeField] private Animator animator;

    [Header("Sound Clips")]
    [SerializeField] private AudioClip hurtClip;

    private float currentHealth;
    private AudioSource hurtAudioSource;
    private bool alive = true;
    private PlayerMovement playerMovement;
    private Transform[] bodypartTransforms;


    public void SendPosition()
    {
        Vector2 rotation = new Vector2(GetCamera().transform.eulerAngles.x, transform.eulerAngles.y);
        Vector3 inputDirection = playerMovement.GetDirection();
        TransformPacket transformPacket = new TransformPacket(NetClient.instance.GetPeerIndex(), transform.position, rotation, inputDirection);
        NetClient.instance.SendPacket(transformPacket);
    }

    public void Update()
    {
        if (NetClient.instance.GetConnected()) {
            SendPosition();
        }


        if (Input.GetKeyDown(KeyCode.E))
        {
            special.RunAttack();
        }


    }


    public void Start()
    {
        Debug.Log(gameObject.name + " Has been instantiated");
        NetClient.instance.SetPlayer(this);
        GameEvent.instance.Spawn(gameObject);
        playerMovement = GetComponent<PlayerMovement>();
        hurtAudioSource = gameObject.AddComponent<AudioSource>();
        currentHealth = maxHealth;
        currentCamera = playerCamera;
    }

    public void SetupPlayer()
    {
        currentHealth = maxHealth;
        alive = true;

    }

    public bool GetAlive() { return alive; }

    public void TakeDamage(ConnectedPeer sender, float damage, float pushbackForce)
    {
        if (!alive)
            return;

        hurtAudioSource.clip = hurtClip;
        hurtAudioSource.Play();

        currentHealth -= damage;
        healthFill.fillAmount -= damage / maxHealth;
        Debug.Log("Damage taken");
        if(currentHealth <= 0)
        {
            DeathInfoPacket packet = new DeathInfoPacket(sender.GetPeerIndex(), pushbackForce);
            NetClient.instance.SendPacket(packet);

            alive = false;
            playerMovement.ActivateRagdoll();
            youDiedText.SetActive(true);
            StartCoroutine(Respawn());

            Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rigidbodies) {
                if(rb != null)
                    rb.AddForce((transform.position - sender.transform.position).normalized * pushbackForce);
            }
            
        }
    }

    public void RegisterKill() {
        skullAnimation.Play();
    }

    public void Heal(float health) {

        currentHealth += health;
        Mathf.Clamp(currentHealth, 0, maxHealth);
        healthFill.fillAmount = currentHealth / maxHealth;
    
    }
    public float GetHealth()
    {
        return currentHealth;
    }
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    public IEnumerator Respawn()
    {
        respawnUI.gameObject.SetActive(true);
        respawnUI.StartCoroutine(respawnUI.RunTimer());
        yield return new WaitForSeconds(5);
        playerMovement.DeactivateRagdoll();
        youDiedText.SetActive(false);
        SetupPlayer();
        GameEvent.instance.LoadPlayer();
    }

    public Camera GetCamera() { return currentCamera; }
    public void ToggleCamera()
    {
        if(currentCamera == playerCamera)
        {
            playerCamera.gameObject.SetActive(false);
            aimCamera.gameObject.SetActive(true);
            currentCamera = aimCamera;
        }
        else
        {
            aimCamera.gameObject.SetActive(false);
            playerCamera.gameObject.SetActive(true);
            currentCamera = playerCamera;
        }
    }


}
