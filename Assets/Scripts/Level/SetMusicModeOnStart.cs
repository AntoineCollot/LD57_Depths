using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMusicModeOnStart : MonoBehaviour
{
    [SerializeField]
    MusicManager.Mode mode;

    // Start is called before the first frame update
    void Start()
    {
        MusicManager.Instance.SetMode(mode);
    }
}
