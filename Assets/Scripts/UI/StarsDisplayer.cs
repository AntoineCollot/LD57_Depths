using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarsDisplayer : MonoBehaviour
{
    [SerializeField] Image[] images;
    [SerializeField] Sprite spriteOn;
    [SerializeField] Sprite spriteOff;
    int currentStarCount;

    // Start is called before the first frame update
    void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.LevelTime > 0)
            SetStars(GameManager.Instance.GetStarsObtained());
    }

    public void SetStars(int count)
    {
        Clear();
        StartCoroutine(SetStarsAnim(count));
    }

    public void Clear()
    {
        foreach (Image image in images)
        {
            image.sprite = spriteOff;
        }
    }

    IEnumerator SetStarsAnim(int count)
    {
        yield return new WaitForSeconds(1.3f);
        for (int i = currentStarCount; i < count; i++)
        {
            images[i].sprite = spriteOn;
            SFXManager.PlaySound(GlobalSFX.GetStar);

            yield return new WaitForSeconds(0.5f);
        }
    }
}
