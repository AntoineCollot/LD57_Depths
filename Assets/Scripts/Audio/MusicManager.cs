using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    AudioSource source;
    AudioLowPassFilter lowPass;
    bool isMuted;

    [Header("Low Pass")]
    [SerializeField] float hiddenFreq;
    [SerializeField] float hiddenResonance;
    [SerializeField] float hiddenVolume;
    public enum Mode { Default, Hidden, MainMenu }
    Mode mode;
    float lowPassAmount01;
    float lowPassRef;

    const float DEFAULT_FREQ = 3000;
    const float DEFAULT_RESONANCE = 1;
    const float DEFAULT_VOLUME = 1f;
    const float LOW_PASS_SMOOTH = 0.15f;

    public static MusicManager Instance;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        lowPass = GetComponent<AudioLowPassFilter>();
    }

    private void Update()
    {
        float target;
        switch (mode)
        {
            case Mode.Default:
            default:
                target = 0;
                break;
            case Mode.Hidden:
                target = 1;
                break;
            case Mode.MainMenu:
                target = 0.8f;
                break;
        }
        lowPassAmount01 = Mathf.SmoothDamp(lowPassAmount01, target, ref lowPassRef, LOW_PASS_SMOOTH);
        lowPass.cutoffFrequency = Mathf.Lerp(DEFAULT_FREQ, hiddenFreq, lowPassAmount01);
        lowPass.lowpassResonanceQ = Mathf.Lerp(DEFAULT_RESONANCE, hiddenResonance, lowPassAmount01);
        source.volume = Mathf.Lerp(DEFAULT_VOLUME, hiddenVolume, lowPassAmount01);
    }

    public void SetMode(Mode mode)
    {
        this.mode = mode;
    }

    public void Mute(bool value)
    {
        isMuted = value;
        source.mute = isMuted;
    }

    public void ToggleMute()
    {
        Mute(!isMuted);
    }
}
