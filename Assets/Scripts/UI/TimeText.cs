using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI nextStarText;
    float originalAlpha;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.onGameStart.AddListener(OnGameStart);
        GameManager.Instance.onGameWin.AddListener(OnGameWin);

        originalAlpha = timeText.color.a;
        Color c = timeText.color;
        c.a = 0;
        timeText.color = c;
        nextStarText.color = c;
    }

    private void OnGameWin()
    {
        int stars = GameManager.Instance.GetStarsObtained(out float nextStarTime);
        if (stars < 3)
        {
            StartCoroutine(FadeIn(nextStarText, 0.5f));
            nextStarText.text = "Next Star: "+nextStarTime.ToString("000.0");
        }
    }

    private void OnGameStart()
    {
        StartCoroutine(FadeIn(timeText,0.5f));
    }

    // Update is called once per frame
    void Update()
    {
        timeText.text = GameManager.Instance.LevelTime.ToString("000.0");
    }

    IEnumerator FadeIn(TextMeshProUGUI text,float time)
    {
        Color c = text.color;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / time;

            c.a = Mathf.Lerp(0, originalAlpha, t);
            text.color = c;

            yield return null;
        }
    }
}
