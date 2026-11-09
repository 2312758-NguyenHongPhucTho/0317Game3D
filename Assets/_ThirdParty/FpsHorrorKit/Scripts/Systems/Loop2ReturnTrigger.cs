using UnityEngine;

namespace FpsHorrorKit
{
    /// <summary>
    /// Place an IsTrigger collider at the ritual-room entrance.
    /// Activates only after the player wakes up in Loop 2.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class Loop2ReturnTrigger : MonoBehaviour
    {
        [SerializeField] private LamAttackCutscene storySequence;
        [SerializeField] private Transform playerRoot;
        private bool armed;
        private bool used;

        private void Awake()
        {
            Collider triggerCollider = GetComponent<Collider>();
            triggerCollider.isTrigger = true;
        }

        public void Arm()
        {
            armed = true;
            used = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!armed || used || storySequence == null || !storySequence.IsLoop2Ready)
                return;
            if (playerRoot == null) return;
            Transform hitTransform = other.transform;
            if (hitTransform != playerRoot && !hitTransform.IsChildOf(playerRoot))
                return;

            used = true;
            storySequence.StartLoop2Ending();
        }

        [ContextMenu("TEST: Trigger Ending (Play Mode, Loop 2 only)")]
        private void TestEnding()
        {
            if (Application.isPlaying && storySequence != null)
                storySequence.StartLoop2Ending();
        }
    }
}
