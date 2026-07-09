using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField, Tooltip("The input reader scriptable object")] private InputReader inputReader;

    [Header("Movement settings")]
    [SerializeField, Tooltip("The speed of the character"), Min(0)] private float speed = 5.0f;
    [SerializeField, Tooltip("The jump height of the character"), Min(0)] private float jumpHeight = 2.0f;
    [SerializeField, Tooltip("Gravity multiplier"), Min(0)] private float gravityMultiplier = 1.0f;

    private CharacterController _characterController;
    private Transform _cameraTransform;
    private Vector3 _velocity;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        if (inputReader != null)
            inputReader.JumpEvent.AddListener(Jump);
    }

    private void OnDisable()
    {
        if (inputReader != null)
            inputReader.JumpEvent.RemoveListener(Jump);
    }

    private void OnValidate()
    {
        if (!inputReader)
        {
            Debug.LogWarning("[MovementController]: InputReader is not assigned.", this);
        }
    }

    private void Update()
    {
        if (inputReader == null) 
            return;

        bool isGrounded = _characterController.isGrounded;
        if (isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        Move(inputReader.MoveInput);

        _velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;

        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void Move(Vector2 input)
    {
        Vector3 move = new Vector3(input.x, 0, input.y);
        move = transform.TransformDirection(move);

        Vector3 direction = (_cameraTransform.forward * input.y) + (_cameraTransform.right * input.x);
        direction.y = 0;
        _velocity.x = direction.x * speed;
        _velocity.z = direction.z * speed;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
        }
    }

    private void Jump()
    {
        if (!_characterController.isGrounded)
            return;

        _velocity.y = Mathf.Sqrt(jumpHeight * -2f * (Physics.gravity.y * gravityMultiplier));
    }
}