using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLantern : MonoBehaviour
{
    [SerializeField, Range(0, 6)] float warningZoneRadius;
    [SerializeField, Range(0, 6)] float flightZoneRadius;

    public enum ProximityZone { TooFar, Warning, Flight }
    HideController hideController;
    public static PlayerLantern Instance;

    public Vector3 LanternPosition => transform.position;
    public bool IsTurnedOn => !hideController.isHidden;


    private void Awake()
    {
        Instance = this;
        hideController = GetComponentInParent<HideController>();
    }

    public float GetDistanceFromLantern(in Vector3 fromPosition)
    {
        Vector3 toLantern = LanternPosition - fromPosition;
        toLantern.Flatten();
        return toLantern.magnitude;
    }

    public ProximityZone GetProximityZone(in Vector3 fromPosition)
    {
        if(!IsTurnedOn)
            return ProximityZone.TooFar;

        float dist = GetDistanceFromLantern(in fromPosition);

        if (dist < flightZoneRadius)
            return ProximityZone.Flight;
        if (dist < flightZoneRadius + warningZoneRadius)
            return ProximityZone.Warning;

        return ProximityZone.TooFar;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(LanternPosition, flightZoneRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(LanternPosition, flightZoneRadius + warningZoneRadius);
    }
#endif
}
