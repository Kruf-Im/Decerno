using KinematicCharacterController;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField, Tooltip("The input reader scriptable object")] private InputReader inputReader;

    [Header("Target Settings")]
    [SerializeField, Tooltip("The pivot point around which camera rotates (Player Transform)")] 
    private Transform target;
    [SerializeField, Tooltip("Vertical offset from target position (e.g., head height)")] 
    private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField, Tooltip("How smoothly the camera tracks the target's position (SmoothDamp time)")]
    private float positionSmoothTime = 0.08f;

    [Header("Camera Settings")]
    [SerializeField, Tooltip("The base distance from the target"), Min(0)]
    private float distance = 5.0f;
    [SerializeField, Tooltip("The speed of camera rotation")]
    private float mouseSensitivity = 120.0f;
    [SerializeField, Range(0, 89)] private float maxPitch = 80.0f;
    [SerializeField, Range(-89, 0)] private float minPitch = -40.0f;
    [SerializeField] private Camera targetCamera;
    [SerializeField, Tooltip("Base Field of View when standing still")]
    private float baseFOV = 60.0f;
    [SerializeField, Tooltip("Max Field of View during high-speed movement or boost")]
    private float maxFOV = 75.0f;
    [SerializeField, Tooltip("Speed threshold where max FOV is reached")]
    private float speedForMaxFOV = 25.0f;
    [SerializeField, Tooltip("How fast FOV adapts to speed changes")]
    private float fovSmoothSpeed = 5.0f;

    [Header("Collision Settings")]
    [SerializeField, Tooltip("Collision check layermask")]
    private LayerMask mask = 1 << 0;
    [SerializeField, Tooltip("Spherecast radius for collision checking")]
    private float cameraRadius = 0.25f;

    private float _pitch = 0.0f;
    private float _yaw = 0.0f;
    private Vector3 _currentFollowPosition;
    private Vector3 _followVelocity;
    private KinematicCharacterMotor _targetMotor;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        var eulerAngles = transform.rotation.eulerAngles;
        _pitch = eulerAngles.x;
        _yaw = eulerAngles.y;

        if (target != null)
        {
            _currentFollowPosition = target.position + targetOffset;
            _targetMotor = target.GetComponent<KinematicCharacterMotor>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (target == null || inputReader == null)
            return;

        UpdateFollowPosition();
        MoveCamera(inputReader.MouseDelta);
        UpdateDynamicFOV();
        CheckCollision();
    }

    private void OnValidate()
    {
        if (!target)
            Debug.LogWarning("[ThirdPersonCameraController]: Target is not assigned.", this);
        if (!inputReader)
            Debug.LogWarning("[ThirdPersonCameraController]: InputReader is not assigned.", this);
    }

    private void UpdateFollowPosition()
    {
        Vector3 targetFocusPoint = target.position + targetOffset;
        _currentFollowPosition = Vector3.SmoothDamp(
            _currentFollowPosition,
            targetFocusPoint,
            ref _followVelocity,
            positionSmoothTime
        );
    }

    private void MoveCamera(Vector2 direction)
    {
        _yaw += direction.x * mouseSensitivity * Time.deltaTime;
        _pitch -= direction.y * mouseSensitivity * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0.0f);

        transform.position = _currentFollowPosition - (transform.forward * distance);
    }

    private void UpdateDynamicFOV()
    {
        if (targetCamera == null) return;

        float currentSpeed = _targetMotor != null
            ? _targetMotor.Velocity.magnitude
            : _followVelocity.magnitude;

        float speedRatio = Mathf.Clamp01(currentSpeed / speedForMaxFOV);
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedRatio);

        targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFOV, fovSmoothSpeed * Time.deltaTime);
    }

    private void CheckCollision()
    {
        Vector3 origin = _currentFollowPosition;
        Vector3 direction = (transform.position - origin).normalized;

        if (Physics.SphereCast(origin, cameraRadius, direction, out RaycastHit hitInfo, distance, mask, QueryTriggerInteraction.Ignore))
        {
            transform.position = origin + direction * Mathf.Max(hitInfo.distance - cameraRadius, 0.1f);
        }
    }
}