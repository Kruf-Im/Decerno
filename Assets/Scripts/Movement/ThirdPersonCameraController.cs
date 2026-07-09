using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField, Tooltip("The input reader scriptable object")] private InputReader inputReader;
    [Header("Camera controller settings")]
    [SerializeField, Tooltip("The pivot point, around which camera rotates")] private Transform target;
    [SerializeField, Tooltip("The distance from the target"), Min(0)] private float distance = 5.0f;
    [SerializeField, Tooltip("The speed of camera rotation")] private float mouseSensitivity = 5.0f;
    [SerializeField, Tooltip("Collision check layermask")] private LayerMask mask = 1 << 0; // Default layer mask
    [SerializeField, Range(0, 89)] private float maxPitch = 80.0f;
    [SerializeField, Range(-89, 0)] private float minPitch = -80.0f;
    private float pitch = 0.0f;
    private float yaw = 0.0f;

    private void Start()
    {
        var eulerAngles = transform.rotation.eulerAngles;
        pitch = eulerAngles.x;
        yaw = eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (target == null || inputReader == null)
            return;
        MoveCamera(inputReader.MouseDelta);
        CheckCollision();
    }
    private void OnValidate()
    {
        if (!target)
        {
            Debug.LogWarning("[ThirdPersonCameraController]: Target is not assigned.", this);
        };
        if (!inputReader)
        {
            Debug.LogWarning("[ThirdPersonCameraController]: InputReader is not assigned.", this);
        }
    }
    private void MoveCamera(Vector2 direction)
    {
        yaw += direction.x * mouseSensitivity * Time.deltaTime;
        pitch -= direction.y * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0.0f);
        transform.position = target.position - transform.forward * distance;
    }
    private void CheckCollision()
    {
        var origin = target.position;
        var direction = (transform.position - target.position).normalized;
        if (Physics.SphereCast(origin, 0.2f, direction, out RaycastHit hitInfo, distance, mask, QueryTriggerInteraction.Ignore))
        {
            transform.position = origin + direction * Mathf.Max(hitInfo.distance - 0.2f, 0.1f);
        }
    }
}
