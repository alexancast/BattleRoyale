using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RespawnUI : MonoBehaviour
{
    [SerializeField] private Image bar;
    [SerializeField] private TextMeshProUGUI count;
    [SerializeField] private float time;

    public IEnumerator RunSeconds()
    {
        for (int i = Mathf.RoundToInt(time); i > 0; i--)
        {
            count.text = i.ToString("0");
            yield return new WaitForSeconds(1);
        }
    }
    public IEnumerator RunTimer()
    {
        StartCoroutine(RunSeconds());
        float animationTime = 0;
        
        while (animationTime < time)
        {
            bar.fillAmount = animationTime / time;
            animationTime += Time.deltaTime;
            yield return null;
        }

        bar.fillAmount = 0;
        gameObject.SetActive(false);
    }
}
