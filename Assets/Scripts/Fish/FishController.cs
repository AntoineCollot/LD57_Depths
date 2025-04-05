using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FishController : MonoBehaviour
{
    public enum State { Patrolling, Afraid, CalmingDown }
    State currentState;

    [Header("Movement")]
    [SerializeField] float movementSmooth = 0.1f;
    [SerializeField] float patrollingSpeed;
    [SerializeField] float flightSpeed;
    Vector3 refMovement;
    Vector3 currentMovement;
    Vector3 targetMovement;
    Rigidbody body;

    [Header("Obstacle Detection")]
    [SerializeField, Range(0, 4)] float obstacleDetectionDistance;
    [SerializeField] int obstacleDetectionRayCount = 12;
    [SerializeField] LayerMask obstacleDetectionLayers;

    Vector3 originalPosition;
    public Vector3 PatrolPosition => originalPosition;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Patrolling:
            default:
                UpdatePatrollingState();
                break;
            case State.Afraid:
                UpdateAfraidState();
                break;
            case State.CalmingDown:
                UpdateCalmingDownState();
                break;
        }

        currentMovement = Vector3.SmoothDamp(currentMovement, targetMovement, ref refMovement, movementSmooth);
        body.velocity = currentMovement;
    }

    void UpdatePatrollingState()
    {
        PlayerLantern.ProximityZone proximity = PlayerLantern.Instance.GetProximityZone(transform.position);
        switch (proximity)
        {
            case PlayerLantern.ProximityZone.TooFar:
            default:
                Patrol();
                break;
            case PlayerLantern.ProximityZone.Warning:
                DisplayWarningForFrame();
                Patrol();
                break;
            case PlayerLantern.ProximityZone.Flight:
                currentState = State.Afraid;
                UpdateAfraidState();
                break;
        }
    }

    void UpdateAfraidState()
    {
        PlayerLantern.ProximityZone proximity = PlayerLantern.Instance.GetProximityZone(transform.position);
        switch (proximity)
        {
            case PlayerLantern.ProximityZone.TooFar:
            default:
                currentState = State.CalmingDown;
                UpdateCalmingDownState();
                break;
            case PlayerLantern.ProximityZone.Warning:
            case PlayerLantern.ProximityZone.Flight:
                RunAway();
                break;
        }
    }

    void UpdateCalmingDownState()
    {
        PlayerLantern.ProximityZone proximity = PlayerLantern.Instance.GetProximityZone(transform.position);
        switch (proximity)
        {
            case PlayerLantern.ProximityZone.TooFar:
            default:
                ReturnToPatrolPosition();
                break;
            case PlayerLantern.ProximityZone.Warning:
                DisplayWarningForFrame();
                ReturnToPatrolPosition();
                break;
            case PlayerLantern.ProximityZone.Flight:
                currentState = State.Afraid;
                UpdateAfraidState();
                break;
        }
    }

    void ReturnToPatrolPosition()
    {
        targetMovement = Vector3.zero;

    }

    void Patrol()
    {
        targetMovement = Vector3.zero;
    }

    void DisplayWarningForFrame()
    {

    }

    void RunAway()
    {
        Vector3 fromLantern = transform.position-PlayerLantern.Instance.LanternPosition;
        fromLantern.Flatten();
        fromLantern.Normalize();
        Debug.Log(fromLantern);

        targetMovement = fromLantern;
        if (TryGetObstacleAvoidanceVector(out Vector3 avoidanceVector))
        {
            targetMovement += avoidanceVector;
        }

        targetMovement.Normalize();
        targetMovement *= flightSpeed;
    }

    bool TryGetObstacleAvoidanceVector(out Vector3 avoidanceVector)
    {
        avoidanceVector = Vector3.zero;

        //Raycast in all directions
        Ray ray = new Ray();
        ray.origin = transform.position;
        float obstacleDetectionRayFloat = obstacleDetectionRayCount;
        RaycastHit hit;
        List<Vector3> hitPoints = new List<Vector3>();
        float py2 = Mathf.PI * 2;
        for (int i = 0; i < obstacleDetectionRayCount; i++)
        {
            ray.direction = new Vector3(Mathf.Cos(i / obstacleDetectionRayFloat * py2), 0, Mathf.Sin(i / obstacleDetectionRayFloat * py2));
            if (Physics.Raycast(ray, out hit, obstacleDetectionDistance, obstacleDetectionLayers))
            {
                hitPoints.Add(hit.point);
            }
        }

        //No avoidance Vector
        if (hitPoints.Count == 0)
        {
            return false;
        }

        //Average opposite direction
        float minDistToObstacle = Mathf.Infinity;
        for (int i = 0; i < hitPoints.Count; i++)
        {
            Vector3 toPoint = hitPoints[i] - transform.position;
            avoidanceVector += toPoint;

            if (toPoint.magnitude < minDistToObstacle)
            {
                minDistToObstacle = toPoint.magnitude;
            }
        }
        avoidanceVector.Flatten();
        avoidanceVector *= -1;
        avoidanceVector.Normalize();

        //Increase size of vector when getting close, Boid things
        avoidanceVector *= Inv(minDistToObstacle,2);

        return true;
    }


    /// <summary>
    /// Nearby objects are prioritized higher than
    ///those further away. This prioritization is described by an inverse square function
    /// </summary>
    float Inv(float x, float s = 1)
    {
        //Avoid dividing by zero using espilon
        float value = x / s + Mathf.Epsilon;
        return 1 / (value * value);
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Ray ray = new Ray();
        ray.origin = transform.position;
        float py2 = Mathf.PI * 2;
        for (int i = 0; i < obstacleDetectionRayCount; i++)
        {
            ray.direction = new Vector3(Mathf.Cos(i / (float)obstacleDetectionRayCount * py2), 0, Mathf.Sin(i / (float)obstacleDetectionRayCount * py2));
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * obstacleDetectionDistance);
        }
    }

    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + Vector3.right, currentState.ToString());
    }
#endif
}
