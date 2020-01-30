using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthKit : MonoBehaviour
{
    [SerializeField] private float healValue = 50;
    [SerializeField] private float spawnTime = 15;
    [SerializeField] private AudioClip healClip;

    private AudioSource audioSource;
    private Renderer meshRenderer;
    private Collider meshCollider;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = healClip;
        meshRenderer = GetComponent<Renderer>();
        meshCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();

            if (player.GetHealth() < player.GetMaxHealth())
            {
                player.Heal(healValue);
                audioSource.Play();
                meshRenderer.enabled = false;
                meshCollider.enabled = false;
                StartCoroutine(Respawn());
            }

        }
    }


    public IEnumerator Respawn()
    {
        yield return new WaitForSeconds(spawnTime);
        meshRenderer.enabled = true;
        meshCollider.enabled = true;
    }
}
