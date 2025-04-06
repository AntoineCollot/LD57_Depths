using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    [SerializeField] float delay;
    [SerializeField] float duration;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeInAnim());
    }

    IEnumerator FadeInAnim()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color c = spriteRenderer.color;
        c.a = 0;
        spriteRenderer.color = c;

        yield return new WaitForSeconds(delay);

        float t = 0;
        while(t<1)
        {
            t += Time.deltaTime / duration;

            c.a = Mathf.Lerp(0, 0.5f, t);
            spriteRenderer.color = c;

            yield return null;
        }
    }
}
