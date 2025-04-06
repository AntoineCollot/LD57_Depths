using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class FishController : MonoBehaviour
{
    public enum State { Patrolling, Afraid, CalmingDown }
    State currentState;

    [Header("Movement")]
    [SerializeField] float movementSmooth = 0.1f;
    [SerializeField] float flightSpeed;
    Vector3 refMovement;
    Vector3 currentMovement;
    Vector3 targetMovement;
    Rigidbody body;
    [SerializeField] float timeBeforeCalming = 1;
    float lastFlightTime;

    [Header("Patrol")]
    [SerializeField] SplineContainer platrolSplineContainer;
    SplinePath splinePath;
    [SerializeField] float calmSpeed;
    Vector3 originalPosition;
    float patrolProgress = 0f;
    float patrolLength;
    const float BACK_TO_PATROL_DIST = 1f;

    [Header("Obstacle Detection")]
    [SerializeField, Range(0, 4)] float obstacleDetectionDistance;
    [SerializeField] int obstacleDetectionRayCount = 12;
    [SerializeField] LayerMask obstacleDetectionLayers;

    public bool MoveDuringPatrol => platrolSplineContainer != null;
    public Vector3 PatrolPosition => originalPosition;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
        body = GetComponent<Rigidbody>();

        if (MoveDuringPatrol)
        {
            InitPatrolPath();
        }
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

    void InitPatrolPath()
    {
        var matrix = platrolSplineContainer.transform.localToWorldMatrix;
        splinePath = new SplinePath(new[]
        {
             new SplineSlice<Spline>(platrolSplineContainer.Splines[0], new SplineRange(0, platrolSplineContainer.Splines[0].Count+1), matrix),
            });
        patrolLength = splinePath.GetLength();

        //Find closest position on spline to start at
        GetClosestProgressOnCurve(transform.position, out patrolProgress, out Vector3 pos);
    }

    void GetClosestProgressOnCurve(Vector3 fromPos, out float progress, out Vector3 nearestPos)
    {
        SplineUtility.GetNearestPoint(splinePath, fromPos, out float3 nearest, out progress);
        nearestPos = nearest;
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
                //Check if enough time has passed since last afraid or warnign
                if (Time.time > lastFlightTime + timeBeforeCalming)
                {
                    currentState = State.CalmingDown;
                    UpdateCalmingDownState();
                }
                else
                    RunAway();
                break;
            case PlayerLantern.ProximityZone.Warning:
            case PlayerLantern.ProximityZone.Flight:
                lastFlightTime = Time.time;
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
        //Stationary patrol
        if (!MoveDuringPatrol)
        {
            targetMovement = originalPosition - transform.position;
            targetMovement.Flatten();
            if (targetMovement.magnitude > 0.01f)
                targetMovement.Normalize();
            targetMovement *= calmSpeed;
            return;
        }

        GetClosestProgressOnCurve(transform.position, out patrolProgress, out Vector3 targetPos);
        targetMovement = targetPos - transform.position;
        targetMovement.Flatten();
        targetMovement.Normalize();
        targetMovement *= calmSpeed;

        //Return to patrol mode
        if (Vector3.Distance(transform.position, targetPos) < BACK_TO_PATROL_DIST)
            currentState = State.Patrolling;
    }

    void Patrol()
    {
        //Stationary patrol
        if (!MoveDuringPatrol)
        {
            targetMovement = Vector3.zero;
            return;
        }

        //Increment progress
        patrolProgress += calmSpeed / patrolLength * Time.deltaTime;
        patrolProgress %= 1;
        Debug.Log(patrolProgress);

        //Evaluate pos
        Vector3 targetPos = splinePath.EvaluatePosition(patrolProgress);

        targetMovement = targetPos - transform.position;
        targetMovement.Flatten();
    }

    void DisplayWarningForFrame()
    {

    }

    void RunAway()
    {
        Vector3 fromLantern = transform.position - PlayerLantern.Instance.LanternPosition;
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
        avoidanceVector *= Inv(minDistToObstacle, 2);

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
