using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float preferredDistance = 10;
    [SerializeField] [Range(0, 100)] private float falloff = 2;
    [SerializeField] private float damage = 10;

    [Header("Feedback")]
    [SerializeField] private AudioClip gunFireClip;
    [SerializeField] private AudioClip hitTargetClip;
    [SerializeField] private ParticleSystem musleFlashParticle;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Animation hitUI;

    [Header("Shooting")]
    [SerializeField] private ShotType shotType;
    [SerializeField] private float cooldown = 1;
    [SerializeField] private float zoom = 1;
    [SerializeField] private TextMeshProUGUI ammunitionText;
    [SerializeField] private int maxAmmunition = 10;
    [SerializeField] private float pushbackForce;
    [SerializeField] private float zoomMouseSensitivity;
    private float regularMouseSensitivity;

    [Header("Reloading")]
    [SerializeField] private float reloadTime = 3; 
    [SerializeField] private Image reloadFill; 
    [SerializeField] private GameObject reloadPanel;
    [SerializeField] private AudioClip reloadClip;

    [Header("Other")]
    [SerializeField] private Player player;

    private bool cooldownActive;
    private bool reloading;
    private AudioSource[] shotAudioSources;
    private AudioSource hitAudioSource;
    private AudioSource reloadAudioSource;
    private int ammunition;
    private PlayerMovement playerMovement;

    public void Start()
    {
        ammunition = maxAmmunition;
        ammunitionText.text = ammunition + "/" + maxAmmunition;

        int audioSourceCount = Mathf.CeilToInt(gunFireClip.length / cooldown);
        for (int i = 0; i < audioSourceCount; i++)
        {
            gameObject.AddComponent<AudioSource>().playOnAwake = false;
        }
        shotAudioSources = GetComponents<AudioSource>();
        hitAudioSource = gameObject.AddComponent<AudioSource>();
        reloadAudioSource = gameObject.AddComponent<AudioSource>();
        playerMovement = player.GetComponent<PlayerMovement>();
        
    }

    public IEnumerator Cooldown()
    {
        cooldownActive = true;
        yield return new WaitForSeconds(cooldown);
        cooldownActive = false;
    }

    public IEnumerator Reload()
    {
        reloading = true;
        reloadPanel.SetActive(true);
        float animationTime = 0;

        while(animationTime < reloadTime)
        {
            reloadFill.fillAmount = animationTime / reloadTime;
            animationTime += Time.deltaTime;
            yield return null;
        }
        reloadFill.fillAmount = 0;
        ammunition = maxAmmunition;
        ammunitionText.text = ammunition + "/" + maxAmmunition;
        reloadAudioSource.clip = reloadClip;
        reloadAudioSource.Play();
        reloading = false;
        reloadPanel.SetActive(false);
    }

    public void Update()
    {

        if (Input.GetMouseButtonDown(1))
        {
            player.ToggleCamera();
            regularMouseSensitivity = playerMovement.GetMouseSensitivity();
            playerMovement.SetMouseSensitivity(zoomMouseSensitivity);
            player.GetCamera().fieldOfView = zoom;

        } else if (Input.GetMouseButtonUp(1)) {
            player.ToggleCamera();
            playerMovement.SetMouseSensitivity(regularMouseSensitivity);
        }
      

        if (!cooldownActive && !reloading && player.GetAlive())
        {
            if (shotType == ShotType.CONTINOUS)
            {
                if (Input.GetMouseButton(0))
                {
                    Fire();
                }

            }
            else if (shotType == ShotType.SINGLE)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Fire();
                }
            }
        }
        
    }

    public float FindDamage(Collider hitCollider, ConnectedPeer hitPeer)
    {
        float distance = Vector3.Distance(transform.position, hitCollider.transform.position);

        if (distance <= preferredDistance)
        {
            return damage * hitPeer.GetHitDamageFallOff(hitCollider.gameObject);
        }
        else
        {
            return Mathf.Clamp((damage - (distance - preferredDistance) * (falloff / 100)), 0, damage) * hitPeer.GetHitDamageFallOff(hitCollider.gameObject);
        }
    }

    public void Fire()
    {

        StartCoroutine(Cooldown());

        for (int i = 0; i < shotAudioSources.Length; i++)
        {
            if (shotAudioSources[i].isPlaying)
            {
                continue;
            }
            else
            {
                shotAudioSources[i].clip = gunFireClip;
                shotAudioSources[i].Play();
                break;
            }
        }
        

        musleFlashParticle.Play();

        RaycastHit hit;


        if (Physics.Raycast(player.GetCamera().transform.position, player.GetCamera().transform.forward, out hit))
        {
            ConnectedPeer hitPeer;

            if (hitPeer = hit.collider.GetComponentInParent<ConnectedPeer>())
            {
                float damage = FindDamage(hit.collider, hitPeer);
                Debug.Log("Hit player: " + hitPeer.GetPeerIndex() + " in the: " + hit.collider.name + ", causing " + damage);
                HitInfoPacket packet = new HitInfoPacket(hitPeer.GetPeerIndex(), damage, pushbackForce);

                if (damage <= 0)
                return;


                if(hitPeer.GetPeerIndex() >= 0)
                {
                    NetClient.instance.SendPacket(packet);

                }

                hitAudioSource.clip = hitTargetClip;
                hitAudioSource.Play();

                hitUI.Stop();
                hitUI.Play();
            }
            else if(hit.collider.GetComponent<IShatterable>() != null){
                hit.collider.GetComponent<IShatterable>().Shatter(pushbackForce);
            }

        }

        ammunition--;
        ammunitionText.text = ammunition + "/" + maxAmmunition;
        if(ammunition <= 0)
        {
            StartCoroutine(Reload());
        }

    }

   

}


public enum ShotType
{
    CONTINOUS, SINGLE
}