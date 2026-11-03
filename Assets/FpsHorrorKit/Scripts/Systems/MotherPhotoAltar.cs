namespace FpsHorrorKit
{
    using UnityEngine;

    /// <summary>
    /// Attach to a photo placement target on the ritual altar.
    /// Calls RitualManager.MarkMotherPhotoUsed() after placing the photo.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class MotherPhotoAltar : MonoBehaviour, IInteractable
    {
        [Header("References")]
        [SerializeField] private MotherPhotoHolder photoHolder;
        [SerializeField] private RitualManager ritualManager;
        [SerializeField] private GameObject placedPhotoVisual;

        [Header("Interaction Text")]
        [SerializeField] private string placeText = "Nhấn [E] để đặt ảnh người mẹ";
        [SerializeField] private string missingText = "Cần tìm ảnh người mẹ";

        private bool _placed;
        private Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();

            if (photoHolder == null)
                photoHolder = FindFirstObjectByType<MotherPhotoHolder>();

            if (ritualManager == null)
                ritualManager = FindFirstObjectByType<RitualManager>();

            if (placedPhotoVisual == gameObject)
            {
                Debug.LogError("[MotherPhotoAltar] Placed Photo Visual must NOT be the altar interaction object.", this);
                placedPhotoVisual = null;
            }

            if (placedPhotoVisual != null)
                placedPhotoVisual.SetActive(false);
        }

        public void Interact()
        {
            if (_placed)
                return;

            if (photoHolder == null)
                photoHolder = FindFirstObjectByType<MotherPhotoHolder>();
            if (ritualManager == null)
                ritualManager = FindFirstObjectByType<RitualManager>();

            if (photoHolder == null || ritualManager == null)
            {
                Debug.LogError("[MotherPhotoAltar] Assign MotherPhotoHolder and RitualManager.", this);
                return;
            }

            if (!photoHolder.HasPhoto || !photoHolder.TryPlace())
            {
                Highlight();
                return;
            }

            _placed = true;
            if (placedPhotoVisual != null)
                placedPhotoVisual.SetActive(true);

            // The existing RitualManager may additionally require the daughter doll to be destroyed.
            ritualManager.MarkMotherPhotoUsed();

            InteractMessageScript.Instance?.ShowMessage("Đã đặt ảnh người mẹ lên bàn thờ.");

            if (_collider != null)
                _collider.enabled = false;

            Debug.Log("[MotherPhotoAltar] Mother's photo used for the ritual.", this);
        }

        public void Highlight()
        {
            if (_placed || PlayerInteract.Instance == null)
                return;

            bool hasPhoto = photoHolder != null && photoHolder.HasPhoto;
            PlayerInteract.Instance.ChangeInteractText(hasPhoto ? placeText : missingText);
        }

        public void HoldInteract() { }
        public void UnHighlight() { }
    }
}
