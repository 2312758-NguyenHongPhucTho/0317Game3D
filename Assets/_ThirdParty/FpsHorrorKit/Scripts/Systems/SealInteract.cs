
namespace FpsHorrorKit
{
    using UnityEngine;

    public class SealInteract : MonoBehaviour, IInteractable
    {
        [Header("Ritual Settings")]
        [SerializeField] private RitualManager ritualManager;

        [SerializeField]
        private RitualManager.DollType dollType =
            RitualManager.DollType.Daughter;

        [Header("Interaction Text")]
        public string readyText = "Nhấn [E] để gỡ phong ấn";

        public string lockedDaughterText =
            "Cần kích hoạt hộp nhạc trước";

        public string lockedMotherText =
            "Cần giải thoát bé gái và dùng ảnh người mẹ";

        private Collider _collider;
        private bool _isBroken;

        private void Awake()
        {
            _collider = GetComponent<Collider>();

            if (ritualManager == null)
            {
                ritualManager =
                    FindAnyObjectByType<RitualManager>();
            }

            if (_collider == null)
            {
                Debug.LogWarning(
                    "[Seal] Missing Collider on " + gameObject.name
                );
            }
        }

        public void Interact()
        {
            if (_isBroken)
                return;

            if (ritualManager == null)
            {
                Debug.LogError(
                    "[Seal] RitualManager not found!"
                );

                return;
            }

            // Check if seal can be removed
            bool canRemove =
                ritualManager.TryBreakSeal(dollType);

            if (!canRemove)
            {
                Debug.Log(
                    "[Seal] Conditions not completed for " +
                    dollType
                );

                Highlight();
                return;
            }

            // Seal is successfully removed
            _isBroken = true;

            if (_collider != null)
            {
                _collider.enabled = false;
            }

            Debug.Log(
                "[Seal] Seal destroyed: " + gameObject.name
            );

            // Destroy only this seal object
            Destroy(gameObject);
        }

        public void Highlight()
        {
            if (_isBroken)
                return;

            if (PlayerInteract.Instance == null)
                return;

            if (ritualManager != null &&
                ritualManager.CanBreakSeal(dollType))
            {
                PlayerInteract.Instance.ChangeInteractText(
                    readyText
                );
            }
            else
            {
                string lockedText =
                    dollType == RitualManager.DollType.Daughter
                    ? lockedDaughterText
                    : lockedMotherText;

                PlayerInteract.Instance.ChangeInteractText(
                    lockedText
                );
            }
        }

        public void HoldInteract()
        {
        }

        public void UnHighlight()
        {
        }
    }
}
