using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEat : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Transform eatOrigin;
    [SerializeField] float eatOriginMaxSpeedOffset = 1;
    [SerializeField] float eatRadius;
    float lastEatTime;
    Vector3 lastEatPosition;
    [SerializeField] LayerMask eatLayers;

    CompositeStateToken isEatingToken;
    [SerializeField] float freezeInputsAfterEatTime = 0.5f;

    [Header("FX")]
    [SerializeField] ParticleSystem particles;

    Vector3 EatPosition
    {
        get
        {
            Vector3 eatDir = eatOrigin.position - transform.position;
            eatDir.Flatten();
            eatDir.Normalize();
            return eatOrigin.position + eatDir * eatOriginMaxSpeedOffset * playerController.NormalizedMoveSpeed;
        }
    }
    HideController hideController;
    PlayerController playerController;

    public float TimeSinceLastEat => Time.time - lastEatTime;

    // Start is called before the first frame update
    void Start()
    {
        hideController = GetComponent<HideController>();
        playerController = GetComponent<PlayerController>();
        hideController.onPlayerHide += OnPlayerHide;
        isEatingToken = new CompositeStateToken();
        PlayerState.Instance.freezeInputsState.Add(isEatingToken);
    }

    private void OnDestroy()
    {
        if(PlayerState.Instance != null)
            PlayerState.Instance.freezeInputsState.Remove(isEatingToken);
    }

    private void OnPlayerHide(bool enter)
    {
        if (enter)
            return;

        lastEatTime = Time.time;
        lastEatPosition = EatPosition;

        Collider[] hitColliders = Physics.OverlapSphere(EatPosition, eatRadius, eatLayers);
        int eatCount = 0;
        foreach (Collider col in hitColliders)
        {
            if(col.TryGetComponent(out FishController fish))
            {
                fish.Die();
                eatCount++;
            }
        }

        if(eatCount > 0)
            SFXManager.PlaySound(GlobalSFX.EatSuccess);
        else
            SFXManager.PlaySound(GlobalSFX.EatFail);

        isEatingToken.SetOn(true);
        Invoke("ResetEatingToken", freezeInputsAfterEatTime);

        particles.Play();
    }

    void ResetEatingToken()
    {
        isEatingToken.SetOn(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Time.time < lastEatTime + 0.3f)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastEatPosition, eatRadius);
        }
        else if(Application.isPlaying && !hideController.isHidden)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(EatPosition, eatRadius);
        }
    }
#endif
}
