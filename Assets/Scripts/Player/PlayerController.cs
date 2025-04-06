using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField, Range(1, 10)] float maxSpeed = 4;
    [SerializeField, Range(1, 10)] float maxSpeedHidden = 7;
    [SerializeField, Range(1, 80)] float maxAcceleration = 20;
    [SerializeField, Range(1, 80)] float maxDeceleration = 20;
    [SerializeField, Range(1, 80)] float maxTurnSpeed = 20;

    float acceleration;
    float deceleration;
    public Vector2 directionInput { get; private set; }
    Vector2 desiredVelocity;
    float turnSpeed;
    float maxSpeedChange;
    Vector2 velocity;

    [Header("Look Rotation")]
    [SerializeField] float lookSmooth = 0.1f;
    Quaternion targetRotation;
    Quaternion currentLookRotation;
    Quaternion refRotation;

    public enum MovementState { Idle, Acceleration, MaxSpeed, Deceleration, Turning }
    public MovementState movementState { get; private set; }

    [Header("Components")]
    Transform camT;
    InputMap inputs;
    Rigidbody body;
    HideController hideController;

    public float NormalizedMoveSpeed
    {
        get
        {
            Vector2 velocity = new Vector2(body.velocity.x, body.velocity.z);
            return velocity.magnitude / EffectiveMaxSpeed;
        }
    }

    public float EffectiveMaxSpeed
    {
        get
        {
            if (hideController.isHidden)
                return maxSpeedHidden;

            return maxSpeed;
        }
    }

    public Vector3 MoveDirection => new Vector3(desiredVelocity.x, 0, desiredVelocity.y).normalized;

    void Awake()
    {
        inputs = new InputMap();
        body = GetComponent<Rigidbody>();
        hideController = GetComponent<HideController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        camT = Camera.main.transform;

        refRotation = Quaternion.identity;
    }

    private void OnEnable()
    {
        inputs.Enable();
    }

    private void OnDisable()
    {
        if (inputs != null)
        {
            inputs.Disable();
        }
    }

    void Update()
    {
        directionInput = inputs.Main.Movement.ReadValue<Vector2>();
        Vector2 effectiveDirectionInput = directionInput;
        //Check if we should freeze the inputs
        if (PlayerState.Instance.freezeInputsState.IsOn || !GameManager.Instance.GameIsPlaying)
        {
            effectiveDirectionInput = Vector2.zero;
        }

        effectiveDirectionInput = AlignWithCamera(effectiveDirectionInput);
        desiredVelocity = effectiveDirectionInput * EffectiveMaxSpeed;

        if (!CameraUtils.IsInCameraBound(Camera.main, transform.position, out Direction outDirection))
        {
            //opposite vector
            switch (outDirection)
            {
                case Direction.Up:
                default:
                    desiredVelocity = Vector2.down;
                    break;
                case Direction.Right:
                    desiredVelocity = Vector2.left;
                    break;
                case Direction.Down:
                    desiredVelocity = Vector2.up;
                    break;
                case Direction.Left:
                    desiredVelocity = Vector2.right;
                    break;
            }

            desiredVelocity *= EffectiveMaxSpeed;
        }
    }

    private void FixedUpdate()
    {
        velocity = new Vector2(body.velocity.x, body.velocity.z);

        Vector2 currentDirectionInput = AlignWithCamera(directionInput);
        //Check if we should freeze the inputs
        if (PlayerState.Instance.freezeInputsState.IsOn)
        {
            currentDirectionInput = Vector2.zero;
        }

        //Select values to use (for now only these)
        acceleration = maxAcceleration;
        deceleration = maxDeceleration;
        turnSpeed = maxTurnSpeed;

        //Movement state
        movementState = MovementState.Idle;

        if (currentDirectionInput.sqrMagnitude > 0.01f)
        {
            if (velocity.sqrMagnitude > 0.01f && Vector2.Dot(velocity, currentDirectionInput) < 0)
            {
                movementState = MovementState.Turning;
            }
            else if (desiredVelocity.magnitude > velocity.magnitude)
            {
                movementState = MovementState.Acceleration;
            }
            else
            {
                movementState = MovementState.MaxSpeed;
            }
        }
        else if (desiredVelocity.magnitude < velocity.magnitude)
        {
            movementState = MovementState.Deceleration;
        }

        //Speed change update based on movement state
        switch (movementState)
        {
            case MovementState.Idle:
            case MovementState.MaxSpeed:
            case MovementState.Acceleration:
                maxSpeedChange = acceleration * Time.fixedDeltaTime;
                break;
            case MovementState.Deceleration:
                maxSpeedChange = deceleration * Time.fixedDeltaTime;
                break;
            case MovementState.Turning:
                maxSpeedChange = turnSpeed * Time.fixedDeltaTime;
                break;
        }

        //Update velocity
        velocity = Vector2.MoveTowards(velocity, desiredVelocity, maxSpeedChange);

        if (PlayerState.Instance.freezePositionState.IsOn)
            velocity = Vector2.zero;

        body.velocity = new Vector3(velocity.x, 0, velocity.y);

        UpdateLookDirection(currentDirectionInput);
    }

    void UpdateLookDirection(Vector2 inputDirection)
    {
        if (PlayerState.Instance.freezeInputsState.IsOn || !GameManager.Instance.GameIsPlaying)
        {
            body.rotation = currentLookRotation.normalized;
            return;
        }

        if (inputDirection.magnitude > 0.01f)
            targetRotation = Quaternion.LookRotation(new Vector3(inputDirection.x, 0, inputDirection.y));

        currentLookRotation = QuaternionUtils.SmoothDamp(currentLookRotation, targetRotation, ref refRotation, lookSmooth).normalized;
        body.rotation = currentLookRotation;
    }

    Vector2 AlignWithCamera(Vector2 vector)
    {
        //Align with camera forward and right
        Vector2 cameraForward = new Vector2(camT.forward.x, camT.forward.z);
        Vector2 cameraRight = new Vector2(camT.right.x, camT.right.z);
        cameraForward.Normalize();
        cameraRight.Normalize();
        return vector.y * cameraForward + vector.x * cameraRight;
    }
}
