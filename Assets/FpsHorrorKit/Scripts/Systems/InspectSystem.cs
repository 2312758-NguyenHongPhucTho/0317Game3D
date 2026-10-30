namespace FpsHorrorKit
{
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class InspectSystem : MonoBehaviour, IInteractable
    {
        [Header("For Rotation")]
        [Tooltip("Object rotation speed")] public float rotationSpeed;
        [Tooltip("Speed for returning to the starting position")] public float returnSpeed = 5f;

        [Header("For Camera")]
        [Tooltip("Distance to the camera during inspection")] public float distanceToCamera;

        [Header("Higlight UI")]
        public string interactText = "Press [E] to Inspect";

        [Header("Inspect Caption")]
        [Tooltip("Optional text shown below this object while it is being inspected.")]
        public string inspectCaption;

        [Header("For Lantern")]
        public Light lanternLight;
        public float lightInspectIntensity;

        [SerializeField] public GameObject PlayerLight;

        private float _startLightIntensity;

        private FpsAssetsInputs _input;
        private FpsController _fpsController;
        private Vector3 _startPosition;
        private Quaternion _startRotation;

        private bool _isInteracting;
        private Collider _collider;
        private bool _playerLightWasActive;
        private bool _playerLightStateSaved;

        private static GameObject _cachedPlayerLight;
        private static GameObject _captionRoot;
        private static TextMeshProUGUI _captionText;
        private static InspectSystem _captionOwner;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _input = FindAnyObjectByType<FpsAssetsInputs>();
            _fpsController = FindAnyObjectByType<FpsController>();
            ResolvePlayerLight();
        }

        private void Start()
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;

            if (lanternLight != null)
                _startLightIntensity = lanternLight.intensity;
        }

        private void LateUpdate()
        {
            if (_isInteracting)
            {
                HandleInspection();
            }
        }

        public void Interact()
        {
            if (_fpsController == null || Camera.main == null) return;

            PlayerInteract.Instance.sendRaycast = false;

            _fpsController.isInteracting = true;
            Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * distanceToCamera;

            StopAllCoroutines();
            StartCoroutine(SmoothTransition(targetPosition, _startRotation, disableCollider: false));

            ToggleCollider(false);
            ToggleLantern(false);
            ShowInspectCaption();
        }

        private void HandleInspection()
        {
            InteractCameraSettings.Instance?.Interacting(distanceToCamera);

            SetPlayerLightActive(true);

            float rotationX = _input.look.x * rotationSpeed * Time.deltaTime;
            float rotationY = _input.look.y * rotationSpeed * Time.deltaTime;

            transform.Rotate(Camera.main.transform.up, -rotationX, Space.World);
            transform.Rotate(Camera.main.transform.right, -rotationY, Space.World);


            if (_input.stopInteract)
            {
                _input.stopInteract = false;
                SetPlayerLightActive(false);
                StopInspection();
            }
        }

        private void StopInspection()
        {
            HideInspectCaption();

            StopAllCoroutines();
            StartCoroutine(SmoothTransition(_startPosition, _startRotation, disableCollider: true));

            _isInteracting = false;
            _fpsController.isInteracting = false;

            InteractCameraSettings.Instance?.NotInteracting();

            ToggleLantern(true);

            PlayerInteract.Instance.sendRaycast = true;
        }

        private void ResolvePlayerLight()
        {
            if (PlayerLight != null)
                return;

            if (_cachedPlayerLight != null)
            {
                PlayerLight = _cachedPlayerLight;
                return;
            }

            Transform[] transforms = FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            foreach (Transform sceneTransform in transforms)
            {
                if (sceneTransform.name != nameof(PlayerLight))
                    continue;

                _cachedPlayerLight = sceneTransform.gameObject;
                PlayerLight = _cachedPlayerLight;
                return;
            }
        }

        private void SetPlayerLightActive(bool isActive)
        {
            if (PlayerLight == null)
                return;

            if (isActive)
            {
                if (!_playerLightStateSaved)
                {
                    _playerLightWasActive = PlayerLight.activeSelf;
                    _playerLightStateSaved = true;
                }

                PlayerLight.SetActive(true);
                return;
            }

            if (_playerLightStateSaved)
            {
                PlayerLight.SetActive(_playerLightWasActive);
                _playerLightStateSaved = false;
            }
        }

        private IEnumerator SmoothTransition(Vector3 targetPosition, Quaternion targetRotation, bool disableCollider)
        {
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f || Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, returnSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, returnSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPosition;
            transform.rotation = targetRotation;

            _isInteracting = !disableCollider;
            ToggleCollider(disableCollider);
        }

        private void ToggleCollider(bool isActive)
        {
            if (_collider != null)
            {
                _collider.enabled = isActive;
            }
            else
            {
                Debug.LogError("Collider not found!");
            }
        }

        private void ToggleLantern(bool isActive)
        {
            if (lanternLight != null)
            {
                lanternLight.intensity = isActive ? _startLightIntensity : lightInspectIntensity;
            }
        }

        public void Highlight()
        {
            PlayerInteract.Instance.ChangeInteractText(interactText);
        }

        private void ShowInspectCaption()
        {
            if (string.IsNullOrWhiteSpace(inspectCaption))
                return;

            EnsureCaptionUI();
            _captionOwner = this;
            _captionText.text = inspectCaption;
            _captionRoot.SetActive(true);
        }

        private void HideInspectCaption()
        {
            if (_captionOwner != this)
                return;

            if (_captionRoot != null)
                _captionRoot.SetActive(false);

            _captionOwner = null;
        }

        private static void EnsureCaptionUI()
        {
            if (_captionRoot != null && _captionText != null)
                return;

            _captionRoot = new GameObject(
                "Inspect Caption UI",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler)
            );

            Canvas canvas = _captionRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1500;

            CanvasScaler scaler = _captionRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject textObject = new GameObject(
                "Caption",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );
            textObject.transform.SetParent(_captionRoot.transform, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -235f);
            rect.sizeDelta = new Vector2(900f, 90f);

            _captionText = textObject.GetComponent<TextMeshProUGUI>();
            _captionText.alignment = TextAlignmentOptions.Center;
            _captionText.fontSize = 34f;
            _captionText.fontStyle = FontStyles.Italic;
            _captionText.color = new Color(0.94f, 0.87f, 0.70f, 1f);
            _captionText.outlineColor = new Color(0.08f, 0.04f, 0.02f, 0.95f);
            _captionText.outlineWidth = 0.24f;
            _captionText.raycastTarget = false;

            _captionRoot.SetActive(false);
        }

        private void OnDisable()
        {
            SetPlayerLightActive(false);
            HideInspectCaption();
        }

        public void HoldInteract() { }
        public void UnHighlight() { }
    }
}
