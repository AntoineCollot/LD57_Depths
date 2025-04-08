using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBakedLowPassForWeb : MonoBehaviour
{
#if UNITY_WEBGL
    MusicManager music;
    AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
        music = GetComponentInParent<MusicManager>();
        music.SetHiddenVolumeMult(0);
    }

    void LateUpdate()
    {
        source.volume = Mathf.Lerp(0, music.hiddenVolume, music.lowPassAmount01);
        if (source.mute != music.isMuted)
            source.mute = music.isMuted;
    }
#endif
}
