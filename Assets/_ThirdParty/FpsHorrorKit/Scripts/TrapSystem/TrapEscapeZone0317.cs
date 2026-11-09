using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace FpsHorrorKit
{
    /// <summary>
    /// Attach to an existing Trap_Trigger with IsTrigger BoxCollider + kinematic Rigidbody.
    /// Replaces DamageTrap0317 on tripwire/bear traps to prevent double damage.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class TrapEscapeZone0317 : MonoBehaviour
    {
        [Header("Damage and escape")]
        [SerializeField, Min(1)] private int damageOnCatch = 30;
        [SerializeField, Min(1)] private int requiredPressesToEscape = 8;
        [SerializeField] private bool lockPlayerInPlace = true;
        [SerializeField] private bool disableTrapAfterEscape = true;
        [SerializeField, Min(0f)] private float rearmDelaySeconds = 1.5f;

        [Header("Look at feet (bear traps)")]
        [SerializeField] private bool lookDownWhenTrapped = true;
        [Tooltip("An Empty transform directly above the closed metal jaws, roughly 0.1m above the floor.")]
        [SerializeField] private Transform cameraLookTarget;

        [Header("Feedback")]
        [SerializeField] private GameObject armedVisual;
        [SerializeField] private GameObject disarmedVisual;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip caughtSound;
        [SerializeField] private AudioClip escapedSound;
        [SerializeField] private UnityEvent onTrapCaught = new UnityEvent();
        [SerializeField] private UnityEvent onTrapEscaped = new UnityEvent();

        private BoxCollider triggerCollider;
        private TrapEscapeHandler0317 capturedPlayer;
        private bool disarmed;
        private bool triggered;
        private bool ready = true;
        private Coroutine rearmRoutine;

        public int RequiredPressesToEscape { get { return Mathf.Max(1, requiredPressesToEscape); } }
        public bool LockPlayerInPlace { get { return lockPlayerInPlace; } }
        public bool LookDownWhenTrapped { get { return lookDownWhenTrapped; } }
        public Transform CameraLookTarget { get { return cameraLookTarget; } }

        private void Awake()
        {
            triggerCollider = GetComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            Rigidbody body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            UpdateVisuals();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!ready || triggered || disarmed) return;
            TrapEscapeHandler0317 handler = other.GetComponentInParent<TrapEscapeHandler0317>();
            PlayerHealth0317 health = other.GetComponentInParent<PlayerHealth0317>();
            if (handler == null || health == null || !health.CanReceiveTrapDamage || handler.IsTrapped || handler.IsReleasing)
                return;

            // One hit only. If another trap caused recent invulnerability, do not trap the player.
            if (!health.TakeTrapDamage(damageOnCatch)) return;
            triggered = true;
            ready = false;
            if (audioSource != null && caughtSound != null) audioSource.PlayOneShot(caughtSound);
            onTrapCaught.Invoke();

            // A fatal trap hit lets PlayerHealth0317 handle its own bed respawn.
            // Do not lock inputs or look down while the death routine is running.
            if (health.IsDeadFromTrap)
            {
                if (disableTrapAfterEscape) DisarmTrap();
                else StartRearm();
                return;
            }

            capturedPlayer = handler;
            if (!handler.TryBeginTrap(this))
            {
                capturedPlayer = null;
                StartRearm();
            }
        }

        public void NotifyEscapeCompleted()
        {
            capturedPlayer = null;
            triggered = false;
            if (audioSource != null && escapedSound != null) audioSource.PlayOneShot(escapedSound);
            onTrapEscaped.Invoke();
            if (disableTrapAfterEscape) DisarmTrap();
            else StartRearm();
        }

        public void NotifyForcedRelease()
        {
            capturedPlayer = null;
            triggered = false;
            if (!disarmed && isActiveAndEnabled) StartRearm();
        }

        private bool waitingForExit;
        private bool exitedSinceCapture;

        private void StartRearm()
        {
            if (rearmRoutine != null) StopCoroutine(rearmRoutine);
            ready = false;
            waitingForExit = true;
            exitedSinceCapture = false;
            rearmRoutine = StartCoroutine(RearmAfterDelay());
        }

        private IEnumerator RearmAfterDelay()
        {
            // Small delay plus an exit/re-entry prevents catching someone still inside.
            yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, rearmDelaySeconds));
            triggered = false;
            rearmRoutine = null;
            if (!disarmed && exitedSinceCapture)
            {
                waitingForExit = false;
                ready = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerHealth0317>() == null) return;
            if (!waitingForExit || disarmed) return;
            exitedSinceCapture = true;
            if (rearmRoutine == null)
            {
                waitingForExit = false;
                ready = true;
            }
        }

        public void DisarmTrap()
        {
            if (rearmRoutine != null) { StopCoroutine(rearmRoutine); rearmRoutine = null; }
            disarmed = true;
            triggered = false;
            ready = false;
            waitingForExit = false;
            exitedSinceCapture = false;
            TrapEscapeHandler0317 p = capturedPlayer;
            capturedPlayer = null;
            if (p != null) p.ForceRelease();
            if (triggerCollider != null) triggerCollider.enabled = false;
            UpdateVisuals();
        }

        public void RearmTrap()
        {
            if (rearmRoutine != null) { StopCoroutine(rearmRoutine); rearmRoutine = null; }
            disarmed = false;
            triggered = false;
            waitingForExit = false;
            exitedSinceCapture = false;
            ready = true;
            if (triggerCollider != null) triggerCollider.enabled = true;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (armedVisual != null) armedVisual.SetActive(!disarmed);
            if (disarmedVisual != null) disarmedVisual.SetActive(disarmed);
        }

        private void OnDisable()
        {
            if (rearmRoutine != null) { StopCoroutine(rearmRoutine); rearmRoutine = null; }
            if (capturedPlayer != null)
            {
                TrapEscapeHandler0317 player = capturedPlayer;
                capturedPlayer = null;
                player.ForceRelease();
            }
        }
    }
}
