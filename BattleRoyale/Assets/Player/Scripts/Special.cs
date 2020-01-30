using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Special : MonoBehaviour
{
    
    [SerializeField] protected Player player;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private float cooldown = 5;
    [SerializeField] private AudioClip notReadyClip;

    protected bool onCooldown;
    protected AudioSource audioSource;


    public void Start()
    {
        CacheComponents();
    }


    public virtual void CacheComponents() { 
    
        audioSource = GetComponent<AudioSource>();
    
    }

    public virtual void RunAttack() {

        if (onCooldown)
        {
            audioSource.clip = notReadyClip;
            audioSource.Play();
            return;
        }
    }

    public virtual IEnumerator Cooldown()
    {

        onCooldown = true;
        float animationTime = 0;

        while (animationTime < cooldown)
        {
            cooldownImage.fillAmount = animationTime / cooldown;
            animationTime += Time.deltaTime;
            yield return null;
        }

        cooldownImage.fillAmount = 1;
        onCooldown = false;
    }
}
