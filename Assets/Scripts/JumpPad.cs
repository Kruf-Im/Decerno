using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("Impulse Settings")]
    [SerializeField, Tooltip("Upward force applied to the player")]
    private float launchForce = 25.0f;

    [SerializeField, Tooltip("Optionally add some forward momentum in the pad's forward direction")]
    private float forwardForce = 0.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out KinematicMovementController player))
        {
            // Build the exact velocity impulse vector
            Vector3 impulse = (transform.up * launchForce) + (transform.forward * forwardForce);

            // Pass the single vector straight into AddForce
            player.AddForce(impulse, ForceMode.Impulse);

            Debug.Log($"[JumpPad]: Launched {other.name} with force {impulse}!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 direction = (transform.up * launchForce) + (transform.forward * forwardForce);
        Gizmos.DrawRay(transform.position, direction);
    }
}