using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILevelWidget : MonoBehaviour
{
    [SerializeField] GameObject[] stars;
    int level;

    public void Init(int level, int starCount)
    {
        this.level = level;
        GetComponentInChildren<TextMeshProUGUI>().text = level.ToString();
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].SetActive(i < starCount);
        }

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        FindObjectOfType<SceneLoader>().LoadLevel(level);
    }
}
