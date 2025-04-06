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
    int defaultMask;

    [SerializeField] float minLanternIntensity = 0.5f;
    float defaultLanternIntensity;
    float defaultLanternRange;

    [Header("Buffer")]
    const float MIN_HIDE_TIME = 0.7f;
    const float EAT_COOLDOWN = 1.3f;
    float lastEatTime;
    float lastHideTime;
    bool? bufferedHideCommand;

    public bool isHidden { get; private set; }

    void Awake()
    {
        inputs = new InputMap();
    }

    private void Start()
    {
        defaultMask = Camera.main.cullingMask;
        defaultLightIntensity = sun.intensity;
        defaultLanternRange = lantern.range;
        defaultLanternIntensity = lantern.intensity;
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

    private void Update()
    {
        if (bufferedHideCommand.HasValue)
        {
            //Cancel buffer if already in this state
            if (bufferedHideCommand.Value == isHidden)
            {
                bufferedHideCommand = null;
            }
            //Hide command buffered
            else if (bufferedHideCommand.Value)
            {
                if (Time.time > lastEatTime + EAT_COOLDOWN)
                {
                    bufferedHideCommand = null;
                    Hide(true);
                }
            }
            //Eat/Unhide command buffered
            else
            {
                if (Time.time > lastHideTime + MIN_HIDE_TIME)
                {
                    bufferedHideCommand = null;
                    Hide(false);
                }
            }
        }
    }

    private void OnHidePressed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        bufferedHideCommand = true;
    }

    private void OnHideCanceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        bufferedHideCommand = false;
    }

    void Hide(bool value)
    {
        isHidden = value;
        onPlayerHide?.Invoke(value);
        StopAllCoroutines();

        if (value)
        {
            lastHideTime = Time.time;
            StartCoroutine(HideTransition(0, 1, 0.2f,0.2f));
        }
        else
        {
            lastEatTime = Time.time;
            StartCoroutine(HideTransition(1, 0, 0.2f));
            Camera.main.cullingMask = defaultMask;
        }
    }

    IEnumerator HideTransition(float from, float to, float time, float delay=0)
    {
        if(delay>0)
            yield return new WaitForSeconds(delay);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / time;

            float hideProgress = Curves.QuadEaseInOut(from, to, Mathf.Clamp01(t));

            sun.intensity = Mathf.Lerp(defaultLightIntensity, hideLightIntensity, hideProgress);
            lantern.range = Mathf.Lerp(defaultLanternRange, 0, hideProgress);
            lantern.intensity = Mathf.Lerp(defaultLanternIntensity, minLanternIntensity, hideProgress);
            Shader.SetGlobalFloat("_DarkFactor", hideProgress);

            yield return null;
        }

        //hide
        if (to > 0.5f)
        {
            Camera.main.cullingMask = hideMask;
        }

    }
}
