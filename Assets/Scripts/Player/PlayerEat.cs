using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEat : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Transform eatOrigin;
    [SerializeField] float eatRadius;
    float lastEatTime;
    Vector3 lastEatPosition;
    [SerializeField] LayerMask eatLayers;

    [Header("FX")]
    [SerializeField] ParticleSystem particles;

    Vector3 EatPosition => eatOrigin.position;
    HideController hideController;

    // Start is called before the first frame update
    void Start()
    {
        hideController = GetComponent<HideController>();
        hideController.onPlayerHide += OnPlayerHide;
    }

    private void OnPlayerHide(bool enter)
    {
        if (enter)
            return;

        lastEatTime = Time.time;
        lastEatPosition = EatPosition;

        Collider[] hitColliders = Physics.OverlapSphere(EatPosition, eatRadius, eatLayers);
        foreach (Collider col in hitColliders)
        {
            Destroy(col.gameObject);
        }

        particles.Play();
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
