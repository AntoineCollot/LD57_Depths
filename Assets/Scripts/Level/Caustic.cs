using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Caustic : MonoBehaviour
{
    [Header("Cookie")]
    [SerializeField] Texture cookieTexture;
    [SerializeField] Material cookieMaterial;

    // Start is called before the first frame update
    void Start()
    {
        //Cookie
        Shader.SetGlobalTexture("_LightCookie", cookieTexture);
        //cookieMaterial.SetFloat("_CookieIntensity",);
        //cookieMaterial.SetFloat("_CloudSpeed", );
    }
}
