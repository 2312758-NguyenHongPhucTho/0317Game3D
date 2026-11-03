using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FpsHorrorKit
{
    public enum TrapActivation0317
    {
        OneShot,            // A bear trap / falling object, damages once per game.
        EachEntry,          // Tripwire / broken floor, damages once each time player enters.
        Continuous          // Fire, shards, poison: repeats while player stands inside.
    }

    /// <summary>
    /// Put on a SEPARATE child object Trap_Trigger with BoxCollider + kinematic Rigidbody.
    /// The script itself handles OnTriggerEnter/Stay/Exit and calls PlayerHealth0317.
    /// Only the player with PlayerHealth0317 can receive damage.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DamageTrap0317 : MonoBehaviour
    {
        [Header("Trap")]
        [SerializeField] private TrapActivation0317 activation = TrapActivation0317.EachEntry;
        [SerializeField, Min(1)] private int damage = 20;
        [Tooltip("For Continuous only: time in seconds between hits.")]
        [SerializeField, Min(0.1f)] private float tickSeconds = 1.0f;
        [SerializeField] private bool activeOnStart = true;

        [Header("Feedback, optional")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip triggerClip;
        [SerializeField] private ParticleSystem triggerParticles;
        [SerializeField] private UnityEvent onTrapActivated = new UnityEvent();

        private BoxCollider trigger;
        private bool isActive;
        private bool used;
        private readonly Dictionary<PlayerHealth0317, int> overlapCounts =
            new Dictionary<PlayerHealth0317, int>();
        private readonly Dictionary<PlayerHealth0317, float> nextTickTime =
            new Dictionary<PlayerHealth0317, float>();

        public bool IsTrapActive { get { return isActive; } }
        public bool HasBeenUsed { get { return used; } }

        private void Awake()
        {
            trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            isActive = activeOnStart;
            trigger.enabled = isActive;
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            PlayerHealth0317 player = other.GetComponentInParent<PlayerHealth0317>();
            if (player == null) return;

            int count;
            overlapCounts.TryGetValue(player, out count);
            overlapCounts[player] = count + 1;
            if (count > 0) return; // Player with multiple colliders counts as one entry.

            if (activation == TrapActivation0317.OneShot && used) return;
            TryDamage(player);
            if (activation == TrapActivation0317.Continuous)
                nextTickTime[player] = Time.time + Mathf.Max(0.1f, tickSeconds);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!isActive || activation != TrapActivation0317.Continuous) return;
            PlayerHealth0317 player = other.GetComponentInParent<PlayerHealth0317>();
            if (player == null) return;
            float next;
            if (!nextTickTime.TryGetValue(player, out next))
            {
                // Handles a player spawned inside a trigger on first physics step.
                nextTickTime[player] = Time.time;
                return;
            }
            if (Time.time < next) return;
            nextTickTime[player] = Time.time + Mathf.Max(0.1f, tickSeconds);
            TryDamage(player);
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerHealth0317 player = other.GetComponentInParent<PlayerHealth0317>();
            if (player == null) return;
            int count;
            if (!overlapCounts.TryGetValue(player, out count)) return;
            if (count > 1) overlapCounts[player] = count - 1;
            else
            {
                overlapCounts.Remove(player);
                nextTickTime.Remove(player);
            }
        }

        private void TryDamage(PlayerHealth0317 player)
        {
            if (activation == TrapActivation0317.OneShot && used) return;
            if (!player.TakeTrapDamage(damage)) return;
            if (audioSource != null && triggerClip != null)
                audioSource.PlayOneShot(triggerClip);
            if (triggerParticles != null) triggerParticles.Play();
            onTrapActivated?.Invoke();
            if (activation == TrapActivation0317.OneShot)
            {
                used = true;
                SetTrapActive(false);
            }
        }

        public void SetTrapActive(bool value)
        {
            isActive = value && !(activation == TrapActivation0317.OneShot && used);
            if (trigger != null) trigger.enabled = isActive;
            overlapCounts.Clear();
            nextTickTime.Clear();
        }

        public void ResetTrap()
        {
            used = false;
            SetTrapActive(true);
        }
    }
}
