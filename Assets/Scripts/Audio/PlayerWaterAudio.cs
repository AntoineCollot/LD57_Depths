using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWaterAudio : MonoBehaviour
{
    AudioSource source;
    PlayerController playerController;
    HideController hideController;

    const float SMOOTH = 0.2f;
    float currentWater;
    float refWater;
    float maxWaterVolume;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        hideController = GetComponentInParent<HideController>();
        source = GetComponent<AudioSource>();
        maxWaterVolume = source.volume;
    }

    // Update is called once per frame
    void Update()
    {
        float targetWater;
        switch (playerController.movementState)
        {
            case PlayerController.MovementState.Idle:
            case PlayerController.MovementState.Deceleration:
            default:
                targetWater = 0;
                break;
            case PlayerController.MovementState.MaxSpeed:
                targetWater = 0.5f;
                break;
            case PlayerController.MovementState.Acceleration:
            case PlayerController.MovementState.Turning:
                targetWater = 1;
                break;
        }

        if (hideController.isHidden)
            targetWater *= 0.3f;
        currentWater = Mathf.SmoothDamp(currentWater, targetWater, ref refWater, SMOOTH);
        source.volume = Mathf.Lerp(0, maxWaterVolume, currentWater);
    }
}
