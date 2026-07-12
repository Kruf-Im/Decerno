using Decerno.Utils;
using KinematicCharacterController;
using UnityEngine;

public class KinematicMovementController : MonoBehaviour, ICharacterController
{
    [Header("Input")]
    [SerializeField, Tooltip("The input reader scriptable object")] private InputReader inputReader;

    [Header("Components")]
    [SerializeField] private KinematicCharacterMotor motor;

    [Header("Movement Settings")]
    [SerializeField] private float MaxGroundSpeed = 8.0f;
    [SerializeField] private float StableMovementAcceleration = 50.0f;
    [SerializeField] private float OrientationSharpness = 15.0f;

    [Header("In Air Settings")]
    [SerializeField] private float MaxAirSpeed = 8.0f;
    [SerializeField] private float AirAcceleration = 20.0f;
    [SerializeField] private float Drag = 0.5f;

    [Header("Jumping & Gravity")]
    //[SerializeField] private InputBuffer jumpBuffer = new();
    [SerializeField] private FloatInputBuffer jumpBuffer = new();
    [SerializeField] private float CoyoteTime = 0.2f;
    [SerializeField] private float MinJumpHeight = 2.0f;
    [SerializeField] private float MaxJumpHeight = 4.0f;
    [SerializeField] private float FullJumpHoldTime = 1f;
    [SerializeField] private Vector3 Gravity = new Vector3(0, -25f, 0);

    private float _coyoteTimeCounter;
    private float _jumpHeldTime;
    private bool _isJumpHeld;

    private Transform _cameraTransform;
    private Vector3 _moveInputVector;
    private Vector3 _lookInputVector;
    private Vector3 _externalForces;

    private void Awake()
    {
        if (motor == null)
            motor = GetComponent<KinematicCharacterMotor>();

        motor.CharacterController = this;

        if (Camera.main != null)
            _cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        if (inputReader != null)
            inputReader.JumpEvent.AddListener(OnJumpEvent);
    }

    private void OnDisable()
    {
        if (inputReader != null)
            inputReader.JumpEvent.RemoveListener(OnJumpEvent);
    }

    private void Update()
    {
        if (inputReader == null || _cameraTransform == null)
            return;
        if (_isJumpHeld)
        {
            _jumpHeldTime += Time.deltaTime;
        }

        Vector2 rawInput = inputReader.MoveInput;

        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        _moveInputVector = (camForward * rawInput.y) + (camRight * rawInput.x);

        if (_moveInputVector.sqrMagnitude > 0.001f)
        {
            _lookInputVector = _moveInputVector;
        }
    }

    private void OnJumpEvent(bool isPressed)
    {
        if (isPressed)
        {
            _isJumpHeld = true;
            _jumpHeldTime = 0f;
            //jumpBuffer.Set();
        }
        else
        {
            _isJumpHeld = false;
            float holdDuration = Mathf.Clamp(_jumpHeldTime, 0.05f, FullJumpHoldTime);
            jumpBuffer.Set(holdDuration);
            _jumpHeldTime = 0f;
        }
    }

    #region Kinematic Character Controller Callbacks
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        bool isGrounded = motor.GroundingStatus.IsStableOnGround;

        if (isGrounded)
        {
            _coyoteTimeCounter = CoyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= deltaTime;
        }

        bool canJump = (isGrounded || _coyoteTimeCounter > 0f);
        // 1. JUMP EXECUTION
        if (canJump && jumpBuffer.TryConsume(out float holdDuration))
        {
            // Calculate jump height using the consumed hold duration
            float holdRatio = Mathf.Clamp01(holdDuration / FullJumpHoldTime);
            float jumpHeight = Mathf.Lerp(MinJumpHeight, MaxJumpHeight, holdRatio);
            float jumpVelocity = Mathf.Sqrt(2f * jumpHeight * -Gravity.y);

            // Reset current Y velocity
            Vector3 verticalVelocity = Vector3.Dot(currentVelocity, motor.CharacterUp) * motor.CharacterUp;
            currentVelocity -= verticalVelocity;
            currentVelocity += motor.CharacterUp * jumpVelocity;

            motor.ForceUnground();
            _coyoteTimeCounter = 0f;

            return;
        }

        // 2. NORMAL MOVEMENT (GROUND VS AIR)
        if (isGrounded)
        {
            Vector3 effectiveMoveInput = motor.GetDirectionTangentToSurface(_moveInputVector, motor.GroundingStatus.GroundNormal);
            Vector3 targetVelocity = effectiveMoveInput * MaxGroundSpeed;

            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, StableMovementAcceleration * deltaTime);
        }
        else
        {
            if (_moveInputVector.sqrMagnitude > 0.001f)
            {
                Vector3 targetAirVelocity = _moveInputVector * MaxAirSpeed;
                Vector3 horizontalVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);

                // Allow high-speed momentum (like explosions/boosters) to be preserved without instantly snapping back to MaxAirSpeed
                if (horizontalVelocity.magnitude <= MaxAirSpeed)
                {
                    horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetAirVelocity, AirAcceleration * deltaTime);
                }

                currentVelocity = horizontalVelocity + (Vector3.Dot(currentVelocity, motor.CharacterUp) * motor.CharacterUp);
            }

            currentVelocity += Gravity * deltaTime;
            currentVelocity *= (1f / (1f + (Drag * deltaTime)));
        }

        // 3. APPLY ACCUMULATED EXTERNAL FORCES & CONSUME
        if (_externalForces.sqrMagnitude > 0.0001f)
        {
            currentVelocity += _externalForces;
            _externalForces = Vector3.zero; // Clear after applying
        }
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (_lookInputVector.sqrMagnitude > 0.001f)
        {
            Vector3 smoothedLookDir = Vector3.Slerp(motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime));
            currentRotation = Quaternion.LookRotation(smoothedLookDir, motor.CharacterUp);
        }
    }

    public void BeforeCharacterUpdate(float deltaTime) { }
    public void PostGroundingUpdate(float deltaTime) { }
    public void AfterCharacterUpdate(float deltaTime) { }
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
    public bool IsColliderValidForCollisions(Collider coll) => true;
    #endregion

    #region Custom Movement Methods
    public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Impulse)
    {
        float mass = motor != null && motor.SimulatedCharacterMass > 0f ? motor.SimulatedCharacterMass : 1.0f;

        switch (forceMode)
        {
            case ForceMode.Force:
                _externalForces += (force / mass) * Time.deltaTime;
                break;

            case ForceMode.Acceleration:
                _externalForces += force * Time.deltaTime;
                break;

            case ForceMode.Impulse:
                _externalForces += force / mass;
                break;

            case ForceMode.VelocityChange:
                Vector3 addedVelocity = (forceMode == ForceMode.Impulse) ? (force / mass) : force;

                Vector3 currentVerticalVel = Vector3.Dot(motor.BaseVelocity, motor.CharacterUp) * motor.CharacterUp;
                if (Vector3.Dot(addedVelocity, motor.CharacterUp) > 0f && Vector3.Dot(currentVerticalVel, motor.CharacterUp) < 0f)
                {
                    motor.BaseVelocity -= currentVerticalVel;
                }

                motor.BaseVelocity += addedVelocity;

                motor.ForceUnground();
                _coyoteTimeCounter = 0f;
                break;
        }

        if (Vector3.Dot(force, motor.CharacterUp) > 0f)
        {
            motor.ForceUnground();
            _coyoteTimeCounter = 0f;
        }
    }
    public void Teleport(Vector3 targetPosition, Quaternion targetRotation)
    {
        motor.SetPositionAndRotation(targetPosition, targetRotation);
    }
    #endregion
}