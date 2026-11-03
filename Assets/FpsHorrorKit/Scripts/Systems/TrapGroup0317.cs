using UnityEngine;

namespace FpsHorrorKit
{
    /// <summary>
    /// Main switch for BOTH ordinary DamageTrap0317 and the E-escape TrapEscapeZone0317.
    /// Keep RitualManager.onRitualCompleted linked to DisableAllTraps().
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrapGroup0317 : MonoBehaviour
    {
        [SerializeField] private DamageTrap0317[] traps;
        [SerializeField] private TrapEscapeZone0317[] escapeTraps;
        [SerializeField] private PlayerHealth0317 playerHealth;

        private void Awake()
        {
            if (traps == null || traps.Length == 0)
                traps = GetComponentsInChildren<DamageTrap0317>(true);
            if (escapeTraps == null || escapeTraps.Length == 0)
                escapeTraps = GetComponentsInChildren<TrapEscapeZone0317>(true);
        }

        public void DisableAllTraps()
        {
            foreach (DamageTrap0317 trap in traps)
                if (trap != null) trap.SetTrapActive(false);
            foreach (TrapEscapeZone0317 trap in escapeTraps)
                if (trap != null) trap.DisarmTrap();
            if (playerHealth != null) playerHealth.SetTrapDamageAllowed(false);
        }

        public void EnableAllTraps()
        {
            if (playerHealth != null) playerHealth.SetTrapDamageAllowed(true);
            foreach (DamageTrap0317 trap in traps)
                if (trap != null) trap.SetTrapActive(true);
            foreach (TrapEscapeZone0317 trap in escapeTraps)
                if (trap != null) trap.RearmTrap();
        }

        public void ResetAndEnableAllTraps()
        {
            if (playerHealth != null) playerHealth.SetTrapDamageAllowed(true);
            foreach (DamageTrap0317 trap in traps)
                if (trap != null) trap.ResetTrap();
            foreach (TrapEscapeZone0317 trap in escapeTraps)
                if (trap != null) trap.RearmTrap();
        }
    }
}
