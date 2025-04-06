using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    Animator anim;
    HideController hideController;
    PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        hideController = GetComponentInParent<HideController>();
        playerController = GetComponentInParent<PlayerController>();
        hideController.onPlayerHide += OnPlayerHide;
    }

    private void OnDestroy()
    {
        if (hideController != null)
            hideController.onPlayerHide -= OnPlayerHide;
    }

    private void Update()
    {
        switch (playerController.movementState)
        {
            case PlayerController.MovementState.Idle:
            case PlayerController.MovementState.MaxSpeed:
            default:
                anim.SetFloat("SwimSpeed", Mathf.Lerp(0.5f, 1.5f, playerController.NormalizedMoveSpeed));
                break;
            case PlayerController.MovementState.Acceleration:
            case PlayerController.MovementState.Deceleration:
            case PlayerController.MovementState.Turning:
                anim.SetFloat("SwimSpeed", Mathf.Lerp(1.5f, 4f, playerController.NormalizedMoveSpeed));
                break;
        }
    }

    private void OnPlayerHide(bool hide)
    {
        anim.SetBool("IsHidden", hide);
    }
}
