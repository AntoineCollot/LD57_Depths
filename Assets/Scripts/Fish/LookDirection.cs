using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookDirection : MonoBehaviour
{
    Rigidbody body;
    Quaternion targetRotation;
    Quaternion currentLookRotation;
    Quaternion refRotation;
    [SerializeField] float lookSmooth = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponentInParent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLookDirection(body.velocity);
    }

    void UpdateLookDirection(Vector3 velocity)
    {
        if (velocity.magnitude < 0.1f)
            return;
        targetRotation = Quaternion.LookRotation(velocity);
        currentLookRotation = QuaternionUtils.SmoothDamp(currentLookRotation, targetRotation, ref refRotation, lookSmooth).normalized;
        transform.LookAt(transform.position + currentLookRotation * Vector3.forward);
    }
}
