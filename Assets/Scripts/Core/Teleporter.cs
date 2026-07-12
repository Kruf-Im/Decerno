using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Teleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField, Tooltip("The destination transform where the player will be moved")]
    private Transform teleportTarget;

    [SerializeField, Tooltip("How long the player must stay inside the zone to teleport")]
    private float chargeTime = 1.0f;

    [SerializeField, Tooltip("Cooldown time before this teleporter can be used again")]
    private float teleportCooldown = 2.0f;

    [Header("Optional Animation / FX")]
    [SerializeField] private Animator animator;

    private Collider _trigger;
    private Coroutine _chargeCoroutine;
    private float _lastTeleportTime = -999f;
    private bool _isCharging = false;

    private static readonly int IsChargingHash = Animator.StringToHash("IsCharging");
    private static readonly int TeleportHash = Animator.StringToHash("Teleport");

    private void Awake()
    {
        _trigger = GetComponent<Collider>();
        _trigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < _lastTeleportTime + teleportCooldown)
            return;

        if (other.TryGetComponent(out KinematicMovementController player))
        {
            _chargeCoroutine = StartCoroutine(ChargeAndTeleportRoutine(player));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out KinematicMovementController player))
        {
            CancelCharge();
        }
    }

    private IEnumerator ChargeAndTeleportRoutine(KinematicMovementController player)
    {
        _isCharging = true;

        if (animator != null)
            animator.SetBool(IsChargingHash, true);

        yield return new WaitForSeconds(chargeTime);
        Teleport(player);
    }

    private void CancelCharge()
    {
        if (_chargeCoroutine != null)
        {
            StopCoroutine(_chargeCoroutine);
            _chargeCoroutine = null;
        }

        _isCharging = false;

        if (animator != null)
            animator.SetBool(IsChargingHash, false);

        Debug.Log($"[Teleporter]: Player left the zone early. Charge canceled!");
    }

    private void Teleport(KinematicMovementController player)
    {
        if (teleportTarget == null)
        {
            Debug.LogError("[Teleporter]: Teleport target is not assigned!", this);
            CancelCharge();
            return;
        }

        player.Teleport(teleportTarget.position, teleportTarget.rotation);

        _lastTeleportTime = Time.time;

        if (animator != null)
        {
            animator.SetBool(IsChargingHash, false);
            animator.SetTrigger(TeleportHash);
        }

        _isCharging = false;
        _chargeCoroutine = null;

        Debug.Log($"[Teleporter]: Player successfully teleported to {teleportTarget.position}!");
    }
}