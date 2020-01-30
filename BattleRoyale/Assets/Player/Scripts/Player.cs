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
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject youDiedText;
    [SerializeField] private RespawnUI respawnUI;
    [SerializeField] private Animation skullAnimation;
    [SerializeField] private Special special;

    [Header("Sound Clips")]
    [SerializeField] private AudioClip hurtClip;

    private float currentHealth;
    private AudioSource hurtAudioSource;
    private bool alive = true;
    private PlayerMovement playerMovement;


    public void SendPosition()
    {
        Vector2 rotation = new Vector2(GetCamera().transform.eulerAngles.x, transform.eulerAngles.y);
        Vector3 inputDirection = playerMovement.GetDirection();
        TransformPacket transformPacket = new TransformPacket(NetCient.instance.GetPeerIndex(), transform.position, rotation, inputDirection);
        NetCient.instance.SendPacket(transformPacket);
    }

    public void Update()
    {
        if (NetCient.instance.GetConnected()) {
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
        Debug.Break();
        NetCient.instance.SetPlayer(this);
        GameEvent.instance.Spawn(gameObject);
        playerMovement = GetComponent<PlayerMovement>();
        currentHealth = maxHealth;
        hurtAudioSource = gameObject.AddComponent<AudioSource>();
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
            NetCient.instance.SendPacket(packet);

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
        GameEvent.instance.LoadPlayer();
    }

    public Camera GetCamera() { return playerCamera; }


}
