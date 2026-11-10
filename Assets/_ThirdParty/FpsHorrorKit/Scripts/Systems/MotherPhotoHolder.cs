namespace FpsHorrorKit
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Keeps the mother's photo without an inventory system.
    /// Attach to the gameplay Player, and assign a visual child of its gameplay camera.
    /// </summary>
    public class MotherPhotoHolder : MonoBehaviour
    {
        [Header("Held Photo (Camera)")]
        [SerializeField] private GameObject heldPhotoVisual;

        [Header("View / Put Away")]
        [SerializeField] private Key togglePhotoKey = Key.F;

        private bool _hasPhoto;
        private bool _isPhotoVisible;
        private bool _photoPlaced;
        private FpsController _fpsController;

        public bool HasPhoto => _hasPhoto;
        public bool IsPhotoVisible => _hasPhoto && _isPhotoVisible;
        public bool PhotoPlaced => _photoPlaced;

        private void Awake()
        {
            _fpsController = GetComponent<FpsController>();
            if (_fpsController == null)
                _fpsController = GetComponentInParent<FpsController>();

            if (heldPhotoVisual == gameObject)
            {
                Debug.LogError("[MotherPhoto] Held Photo Visual cannot be Player itself.", this);
                heldPhotoVisual = null;
            }

            UpdatePhotoVisual();
        }

        private void OnEnable()
        {
            UpdatePhotoVisual();
        }

        private void Update()
        {
            if (!_hasPhoto || Time.timeScale <= 0f || Keyboard.current == null)
                return;

            // Do not change photos during the FpsHorrorKit item-inspection mode.
            if (_fpsController != null && _fpsController.isInteracting)
                return;

            if (Keyboard.current[togglePhotoKey].wasPressedThisFrame)
            {
                _isPhotoVisible = !_isPhotoVisible;
                UpdatePhotoVisual();
            }
        }

        public bool TryPickup()
        {
            if (_hasPhoto || _photoPlaced)
                return false;

            if (heldPhotoVisual == null)
            {
                Debug.LogWarning("[MotherPhoto] Assign Held Photo Visual on MotherPhotoHolder before picking up.", this);
                return false;
            }

            _hasPhoto = true;
            _isPhotoVisible = true;
            UpdatePhotoVisual();
            Debug.Log("[MotherPhoto] Picked up the mother's photo.", this);
            return true;
        }

        public bool TryPlace()
        {
            if (!_hasPhoto || _photoPlaced)
                return false;

            _hasPhoto = false;
            _isPhotoVisible = false;
            _photoPlaced = true;
            UpdatePhotoVisual();
            Debug.Log("[MotherPhoto] Placed the mother's photo on the altar.", this);
            return true;
        }

        private void UpdatePhotoVisual()
        {
            if (heldPhotoVisual != null)
                heldPhotoVisual.SetActive(_hasPhoto && _isPhotoVisible);
        }

        private void OnDisable()
        {
            if (heldPhotoVisual != null)
                heldPhotoVisual.SetActive(false);
        }
    }
}
