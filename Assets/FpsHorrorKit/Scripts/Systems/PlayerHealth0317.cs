using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace FpsHorrorKit
{
    /// <summary>
    /// Standalone health for traps. At 0 HP from a trap, respawn at bed/initial
    /// location WITHOUT reloading the scene (ritual dolls remain destroyed).
    /// Cinematic damage uses SetHealthForCutscene and does not trigger trap death.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHealth0317 : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField, Min(0f)] private float damageInvulnerabilitySeconds = 0.45f;

        [Header("Death caused by traps ONLY")]
        [SerializeField] private bool autoRespawnAfterTrapDeath = true;
        [SerializeField, Min(0f)] private float trapDeathDelay = 2.5f;
        [Tooltip("Usually BedSpawn. If empty, uses player's initial scene position.")]
        [SerializeField] private Transform bedSpawn;
        [SerializeField] private CharacterController characterController;
        [Tooltip("Player move/look/interact behaviours to temporarily disable on trap death. Do not add this Health component.")]
        [SerializeField] private Behaviour[] playerControlScripts;

        [Header("Optional event hooks")]
        public UnityEvent onTrapDeath = new UnityEvent();
        public UnityEvent onTrapRespawn = new UnityEvent();

        public event Action<int, int> HealthChanged;
        public event Action<int> TrapDamaged;

        private int currentHealth;
        private bool deadFromTrap;
        private bool trapDamageAllowed = true;
        private float nextAllowedDamageTime;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Coroutine respawnCoroutine;

        public int CurrentHealth { get { return currentHealth; } }
        public int MaxHealth { get { return Mathf.Max(1, maxHealth); } }
        public float HealthNormalized { get { return (float)currentHealth / MaxHealth; } }
        public bool IsDeadFromTrap { get { return deadFromTrap; } }
        public bool CanReceiveTrapDamage { get { return trapDamageAllowed && !deadFromTrap; } }

        private void Awake()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            currentHealth = maxHealth;
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        /// <returns>True only when actual damage was applied.</returns>
        public bool TakeTrapDamage(int damage)
        {
            if (!CanReceiveTrapDamage || damage <= 0 || currentHealth <= 0 ||
                Time.time < nextAllowedDamageTime)
                return false;

            int oldHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            nextAllowedDamageTime = Time.time + damageInvulnerabilitySeconds;
            HealthChanged?.Invoke(currentHealth, MaxHealth);
            TrapDamaged?.Invoke(oldHealth - currentHealth);

            if (currentHealth <= 0)
            {
                deadFromTrap = true;
                onTrapDeath?.Invoke();
                if (autoRespawnAfterTrapDeath && respawnCoroutine == null)
                    respawnCoroutine = StartCoroutine(TrapRespawnRoutine());
            }
            return true;
        }

        [ContextMenu("TEST (Play Mode): Take 20 Trap Damage")]
        private void TestDamage()
        {
            if (Application.isPlaying) TakeTrapDamage(20);
        }

        [ContextMenu("TEST (Play Mode): Full Heal")]
        private void TestRestore()
        {
            if (Application.isPlaying) RestoreFullHealth();
        }

        public bool Heal(int amount)
        {
            if (deadFromTrap || amount <= 0 || currentHealth >= MaxHealth) return false;
            currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
            HealthChanged?.Invoke(currentHealth, MaxHealth);
            return true;
        }

        /// <summary>
        /// For LamAttackCutscene's three scripted hits: updates the HP bar ONLY.
        /// Does NOT invoke trap-death/respawn. Use RestoreFullHealth on Loop 2.
        /// </summary>
        public void SetHealthForCutscene(int health)
        {
            if (deadFromTrap) return;
            currentHealth = Mathf.Clamp(health, 0, MaxHealth);
            HealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        public void RestoreFullHealth()
        {
            if (respawnCoroutine != null)
            {
                StopCoroutine(respawnCoroutine);
                respawnCoroutine = null;
            }
            deadFromTrap = false;
            currentHealth = MaxHealth;
            nextAllowedDamageTime = Time.time + 0.75f;
            HealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        public void SetTrapDamageAllowed(bool allow)
        {
            trapDamageAllowed = allow;
        }

        public void SetBedSpawn(Transform value)
        {
            bedSpawn = value;
        }

        private IEnumerator TrapRespawnRoutine()
        {
            // Only suspend scripts which are currently enabled; preserve their states.
            int len = playerControlScripts == null ? 0 : playerControlScripts.Length;
            bool[] previous = new bool[len];
            for (int i = 0; i < len; i++)
            {
                if (playerControlScripts[i] == null) continue;
                previous[i] = playerControlScripts[i].enabled;
                playerControlScripts[i].enabled = false;
            }

            yield return new WaitForSecondsRealtime(trapDeathDelay);

            bool controllerWasEnabled = characterController != null && characterController.enabled;
            if (characterController != null) characterController.enabled = false;
            Transform destination = bedSpawn;
            transform.SetPositionAndRotation(
                destination != null ? destination.position : initialPosition,
                destination != null ? destination.rotation : initialRotation);
            if (characterController != null) characterController.enabled = controllerWasEnabled;

            // Restore explicitly: do not start another death/Loop 2 cutscene.
            deadFromTrap = false;
            currentHealth = MaxHealth;
            nextAllowedDamageTime = Time.time + 1f;
            HealthChanged?.Invoke(currentHealth, MaxHealth);
            for (int i = 0; i < len; i++)
                if (playerControlScripts[i] != null)
                    playerControlScripts[i].enabled = previous[i];
            respawnCoroutine = null;
            onTrapRespawn?.Invoke();
        }
    }
}
