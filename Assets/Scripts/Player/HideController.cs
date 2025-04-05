using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideController : MonoBehaviour
{
    InputMap inputs;
    public event Action<bool> onPlayerHide;
    [SerializeField] LayerMask hideMask;

    [Header("Light")]
    [SerializeField] Light sun;
    [SerializeField] Light lantern;
    float defaultLightIntensity = 0.1f;
    [SerializeField] float hideLightIntensity = 0.05f;
    int cullingMask;

    public bool isHidden { get; private set; }

    void Awake()
    {
        inputs = new InputMap();
    }

    private void Start()
    {
        cullingMask = Camera.main.cullingMask;
        defaultLightIntensity = sun.intensity;
    }

    private void OnEnable()
    {
        inputs.Enable();
        inputs.Main.Hide.performed += OnHidePressed;
        inputs.Main.Hide.canceled += OnHideCanceled;
    }

    private void OnDisable()
    {
        if (inputs != null)
        {
            inputs.Disable();

            inputs.Main.Hide.performed += OnHidePressed;
            inputs.Main.Hide.canceled += OnHideCanceled;
        }
    }

    private void OnHidePressed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isHidden = true;
        Camera.main.cullingMask = hideMask;
        onPlayerHide?.Invoke(true);
        lantern.enabled = false;
        sun.intensity = hideLightIntensity;
    }

    private void OnHideCanceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isHidden = false;
        Camera.main.cullingMask = cullingMask;
        onPlayerHide?.Invoke(false);
        lantern.enabled = true;
        sun.intensity = defaultLightIntensity;
    }
}
