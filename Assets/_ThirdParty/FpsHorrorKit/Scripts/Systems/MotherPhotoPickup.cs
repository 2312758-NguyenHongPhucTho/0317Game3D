namespace FpsHorrorKit
{
    using UnityEngine;

    /// <summary>
    /// Attach to the photo lying on a table (must have a non-trigger Collider).
    /// PlayerInteract calls Interact() through IInteractable when E is pressed.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class MotherPhotoPickup : MonoBehaviour, IInteractable
    {
        [Header("References")]
        [SerializeField] private MotherPhotoHolder photoHolder;

        [Header("Interaction Text")]
        [SerializeField] private string pickupText = "Nhấn [E] để nhặt ảnh người mẹ";

        private bool _pickedUp;

        private void Awake()
        {
            if (photoHolder == null)
                photoHolder = FindFirstObjectByType<MotherPhotoHolder>();
        }

        public void Interact()
        {
            if (_pickedUp)
                return;

            if (photoHolder == null)
                photoHolder = FindFirstObjectByType<MotherPhotoHolder>();

            if (photoHolder == null)
            {
                Debug.LogError("[MotherPhotoPickup] MotherPhotoHolder not found on Player.", this);
                return;
            }

            if (!photoHolder.TryPickup())
                return;

            _pickedUp = true;
            // Keep scene setup intact; hide only the world photo.
            gameObject.SetActive(false);
            InteractMessageScript.Instance?.ShowMessage("Đã nhặt ảnh người mẹ. Nhấn [F] để xem hoặc cất ảnh.");
        }

        public void Highlight()
        {
            if (!_pickedUp && PlayerInteract.Instance != null)
                PlayerInteract.Instance.ChangeInteractText(pickupText);
        }

        public void HoldInteract() { }
        public void UnHighlight() { }
    }
}
