using UnityEngine;
using UnityEngine.Events;

namespace FpsHorrorKit
{
    /// <summary>
    /// Small adapter for the existing ritual-room door. Assign real colliders
    /// and interaction scripts manually; no assumptions about the door system.
    /// </summary>
    public sealed class RitualRoomDoorLock : MonoBehaviour
    {
        [Header("Door references")]
        [Tooltip("Scripts which open/close the door via E. They are disabled during chase.")]
        [SerializeField] private Behaviour[] doorInteractionScripts;
        [Tooltip("DEDICATED invisible barrier non-trigger colliders, not the original door colliders.")]
        [SerializeField] private Collider[] blockingColliders;
        [Tooltip("Keep dedicated barriers disabled until the chase begins.")]
        [SerializeField] private bool disableBarriersOnAwake = true;

        [Header("Optional door pose")]
        [Tooltip("Only enable if door panel's actual pivot and angles are verified.")]
        [SerializeField] private bool controlDoorRotation = false;
        [SerializeField] private Transform doorLeaf;
        [SerializeField] private Vector3 closedLocalEuler;
        [SerializeField] private Vector3 openLocalEuler;

        [Header("Optional hooks for existing door animation or locking APIs")]
        public UnityEvent onDoorLocked = new UnityEvent();
        public UnityEvent onDoorOpenedForLoop2 = new UnityEvent();

        private bool locked;

        private void Awake()
        {
            if (disableBarriersOnAwake) SetBlockersEnabled(false);
        }

        public void LockDoor()
        {
            locked = true;
            if (controlDoorRotation && doorLeaf != null)
                doorLeaf.localRotation = Quaternion.Euler(closedLocalEuler);
            SetInteractionsEnabled(false);
            SetBlockersEnabled(true);
            onDoorLocked?.Invoke();
            Debug.Log("[03:17] Ritual room door LOCKED.");
        }

        public void OpenForLoop2()
        {
            locked = false;
            if (controlDoorRotation && doorLeaf != null)
                doorLeaf.localRotation = Quaternion.Euler(openLocalEuler);
            // Do not let legacy door scripts automatically close it during Loop 2.
            SetInteractionsEnabled(false);
            SetBlockersEnabled(false);
            onDoorOpenedForLoop2?.Invoke();
            Debug.Log("[03:17] Ritual room doorway opened for Loop 2.");
        }

        private void SetInteractionsEnabled(bool value)
        {
            if (doorInteractionScripts == null) return;
            foreach (Behaviour behaviour in doorInteractionScripts)
                if (behaviour != null) behaviour.enabled = value;
        }

        private void SetBlockersEnabled(bool value)
        {
            if (blockingColliders == null) return;
            foreach (Collider collider in blockingColliders)
                if (collider != null) collider.enabled = value;
        }

        public bool IsLocked { get { return locked; } }
    }
}
