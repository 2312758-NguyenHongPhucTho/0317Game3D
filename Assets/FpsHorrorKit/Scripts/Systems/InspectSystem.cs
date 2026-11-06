
namespace FpsHorrorKit
{
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;

    public class InspectSystem : MonoBehaviour, IInteractable
    {
        [Header("For Rotation")]
        [Tooltip("Object rotation speed")]
        public float rotationSpeed;

        [Tooltip("Speed for returning to starting position")]
        public float returnSpeed = 5f;

        [Header("For Camera")]
        [Tooltip("Distance to camera during inspection")]
        public float distanceToCamera;

        [Header("Highlight UI")]
        public string interactText = "Nhấn [E] để kiểm tra";

        [Header("Inspect Caption")]
        public string inspectCaption;

        [Header("For Lantern")]
        public Light lanternLight;
        public float lightInspectIntensity;

        [SerializeField]
        public GameObject PlayerLight;

        [Header("Destroy Item")]
        public bool canDestroyWithX = true;

        [Header("Ritual Doll")]
        [Tooltip("Enable only for story ritual dolls")]
        public bool isRitualDoll = false;

        public RitualManager ritualManager;

        public RitualManager.DollType dollType =
            RitualManager.DollType.Daughter;

        private float _startLightIntensity;

        private FpsAssetsInputs _input;
        private FpsController _fpsController;

        private Vector3 _startPosition;
        private Quaternion _startRotation;

        private bool _isInteracting;
        private bool _inspectionSessionActive;
        private bool _isBeingDestroyed;

        private Collider _collider;

        private bool _playerLightWasActive;
        private bool _playerLightStateSaved;

        private static GameObject _cachedPlayerLight;

        private static GameObject _captionRoot;
        private static TextMeshProUGUI _captionText;
        private static InspectSystem _captionOwner;

        // Thông báo khi vật phẩm đã được trả về đúng vị trí sau thao tác Q.
        // Chỉ radio nào có RadioInspectMusic0317 đăng ký mới phát nhạc.
        public event System.Action InspectionReturned;

        private void Awake()
        {
            _collider = GetComponent<Collider>();

            _input = FindAnyObjectByType<FpsAssetsInputs>();

            _fpsController =
                FindAnyObjectByType<FpsController>();

            ResolvePlayerLight();

            if (isRitualDoll && ritualManager == null)
            {
                ritualManager =
                    FindAnyObjectByType<RitualManager>();
            }
        }

        private void Start()
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;

            if (lanternLight != null)
            {
                _startLightIntensity =
                    lanternLight.intensity;
            }
        }

        private void LateUpdate()
        {
            if (_isInteracting && !_isBeingDestroyed)
            {
                HandleInspection();
            }
        }

        // =====================================
        // START INSPECTION
        // =====================================

        public void Interact()
        {
            if (_isBeingDestroyed ||
                _inspectionSessionActive)
            {
                return;
            }

            if (_fpsController == null ||
                Camera.main == null)
            {
                return;
            }

            _inspectionSessionActive = true;
            _isInteracting = false;

            if (PlayerInteract.Instance != null)
            {
                PlayerInteract.Instance.sendRaycast = false;
            }

            _fpsController.isInteracting = true;

            Vector3 targetPosition =
                Camera.main.transform.position +
                Camera.main.transform.forward *
                distanceToCamera;

            StopAllCoroutines();

            ToggleCollider(false);
            ToggleLantern(false);

            ShowInspectCaption();

            StartCoroutine(
                SmoothTransition(
                    targetPosition,
                    _startRotation,
                    false
                )
            );
        }

        // =====================================
        // INSPECT INPUT
        // =====================================

        private void HandleInspection()
        {
            // Press X to destroy item
            if (canDestroyWithX &&
                Keyboard.current != null &&
                Keyboard.current.xKey.wasPressedThisFrame)
            {
                if (!CanDestroyItem())
                {
                    ShowInspectMessage(
                    "Búp bê vẫn còn bị phong ấn..."
                    );

                    Debug.Log(
                        "[Inspect] Doll is still sealed!"
                    );
                }
                else
                {
                    DestroyInspectedItem();
                    return;
                }
            }

            InteractCameraSettings.Instance?.
                Interacting(distanceToCamera);

            SetPlayerLightActive(true);

            if (_input != null && Camera.main != null)
            {
                float rotationX =
                    _input.look.x *
                    rotationSpeed *
                    Time.deltaTime;

                float rotationY =
                    _input.look.y *
                    rotationSpeed *
                    Time.deltaTime;

                transform.Rotate(
                    Camera.main.transform.up,
                    -rotationX,
                    Space.World
                );

                transform.Rotate(
                    Camera.main.transform.right,
                    -rotationY,
                    Space.World
                );
            }

            if (_input != null && _input.stopInteract)
            {
                _input.stopInteract = false;

                StopInspection();
            }
        }

        // =====================================
        // CHECK DESTROY PERMISSION
        // =====================================

        private bool CanDestroyItem()
        {
            if (!canDestroyWithX)
                return false;

            // Normal item can be destroyed
            if (!isRitualDoll)
                return true;

            // Ritual doll needs RitualManager
            if (ritualManager == null)
                return false;

            // Ritual doll must have its seal removed
            return ritualManager.IsSealBroken(dollType);
        }

        // =====================================
        // DESTROY INSPECTED ITEM
        // =====================================

        private void DestroyInspectedItem()
        {
            if (_isBeingDestroyed)
                return;

            // Notify RitualManager before destroying
            if (isRitualDoll)
            {
                if (ritualManager == null)
                {
                    Debug.LogError(
                        "[Inspect] RitualManager missing!"
                    );

                    return;
                }

                bool accepted =
                    ritualManager.NotifyDollDestroyed(dollType);

                if (!accepted)
                {
                    Debug.LogWarning(
                        "[Inspect] Doll destroy rejected."
                    );

                    return;
                }
            }

            _isBeingDestroyed = true;
            _isInteracting = false;
            _inspectionSessionActive = false;

            StopAllCoroutines();

            HideInspectCaption();

            SetPlayerLightActive(false);

            ToggleLantern(true);

            if (_fpsController != null)
            {
                _fpsController.isInteracting = false;
            }

            InteractCameraSettings.Instance?.
                NotInteracting();

            if (PlayerInteract.Instance != null)
            {
                PlayerInteract.Instance.sendRaycast = true;
            }

            if (_input != null)
            {
                _input.stopInteract = false;
            }

            ToggleCollider(false);

            Debug.Log(
                "[Inspect] Item destroyed: " +
                gameObject.name
            );

            Destroy(gameObject);
        }

        // =====================================
        // STOP INSPECTION
        // =====================================

        private void StopInspection()
        {
            _isInteracting = false;
            _inspectionSessionActive = false;

            HideInspectCaption();

            SetPlayerLightActive(false);

            StopAllCoroutines();

            StartCoroutine(
                SmoothTransition(
                    _startPosition,
                    _startRotation,
                    true
                )
            );

            if (_fpsController != null)
            {
                _fpsController.isInteracting = false;
            }

            InteractCameraSettings.Instance?.
                NotInteracting();

            ToggleLantern(true);

            if (PlayerInteract.Instance != null)
            {
                PlayerInteract.Instance.sendRaycast = true;
            }
        }

        // =====================================
        // PLAYER LIGHT
        // =====================================

        private void ResolvePlayerLight()
        {
            if (PlayerLight != null)
                return;

            if (_cachedPlayerLight != null)
            {
                PlayerLight = _cachedPlayerLight;
                return;
            }

            Transform[] transforms =
                FindObjectsByType<Transform>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (Transform sceneTransform in transforms)
            {
                if (sceneTransform.name !=
                    nameof(PlayerLight))
                {
                    continue;
                }

                _cachedPlayerLight =
                    sceneTransform.gameObject;

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
                    _playerLightWasActive =
                        PlayerLight.activeSelf;

                    _playerLightStateSaved = true;
                }

                PlayerLight.SetActive(true);
                return;
            }

            if (_playerLightStateSaved)
            {
                PlayerLight.SetActive(
                    _playerLightWasActive
                );

                _playerLightStateSaved = false;
            }
        }

        // =====================================
        // SMOOTH TRANSITION
        // =====================================

        private IEnumerator SmoothTransition(
            Vector3 targetPosition,
            Quaternion targetRotation,
            bool enableColliderWhenFinished
        )
        {
            float speed = Mathf.Max(
                0.01f,
                returnSpeed
            );

            while (
                Vector3.Distance(
                    transform.position,
                    targetPosition
                ) > 0.01f ||

                Quaternion.Angle(
                    transform.rotation,
                    targetRotation
                ) > 0.1f
            )
            {
                float t = Mathf.Clamp01(
                    speed * Time.deltaTime
                );

                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    t
                );

                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    targetRotation,
                    t
                );

                yield return null;
            }

            transform.position = targetPosition;
            transform.rotation = targetRotation;

            _isInteracting =
                !enableColliderWhenFinished;

            ToggleCollider(
                enableColliderWhenFinished
            );

            // Chỉ phát sự kiện khi Q trả vật phẩm XONG, không phát khi E nhấc lên.
            if (enableColliderWhenFinished)
                InspectionReturned?.Invoke();
        }

        private void ToggleCollider(bool isActive)
        {
            if (_collider != null)
            {
                _collider.enabled = isActive;
            }
            else
            {
                Debug.LogWarning(
                    "[Inspect] Collider not found!"
                );
            }
        }

        private void ToggleLantern(bool isActive)
        {
            if (lanternLight != null)
            {
                lanternLight.intensity = isActive
                    ? _startLightIntensity
                    : lightInspectIntensity;
            }
        }

        // =====================================
        // INTERACTION UI
        // =====================================

        public void Highlight()
        {
            if (_isBeingDestroyed)
                return;

            if (PlayerInteract.Instance != null)
            {
                PlayerInteract.Instance.ChangeInteractText(
                    interactText
                );
            }
        }

        // =====================================
        // INSPECT CAPTION
        // =====================================

        private void ShowInspectCaption()
        {
            if (string.IsNullOrWhiteSpace(inspectCaption))
                return;

            ShowInspectMessage(inspectCaption);
        }

        private void ShowInspectMessage(string message)
        {
            EnsureCaptionUI();

            _captionOwner = this;

            _captionText.text = message;

            _captionRoot.SetActive(true);
        }

        private void HideInspectCaption()
        {
            if (_captionOwner != this)
                return;

            if (_captionRoot != null)
            {
                _captionRoot.SetActive(false);
            }

            _captionOwner = null;
        }

        private static void EnsureCaptionUI()
        {
            if (_captionRoot != null &&
                _captionText != null)
            {
                return;
            }

            _captionRoot = new GameObject(
                "Inspect Caption UI",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler)
            );

            Canvas canvas =
                _captionRoot.GetComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 1500;

            CanvasScaler scaler =
                _captionRoot.GetComponent<CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(1920f, 1080f);

            scaler.matchWidthOrHeight = 0.5f;

            GameObject textObject = new GameObject(
                "Caption",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            textObject.transform.SetParent(
                _captionRoot.transform,
                false
            );

            RectTransform rect =
                textObject.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                new Vector2(0f, -235f);

            rect.sizeDelta =
                new Vector2(900f, 90f);

            _captionText =
                textObject.GetComponent<TextMeshProUGUI>();

            _captionText.alignment =
                TextAlignmentOptions.Center;

            _captionText.fontSize = 34f;

            _captionText.fontStyle =
                FontStyles.Italic;

            _captionText.color =
                new Color(
                    0.94f,
                    0.87f,
                    0.70f,
                    1f
                );

            _captionText.outlineColor =
                new Color(
                    0.08f,
                    0.04f,
                    0.02f,
                    0.95f
                );

            _captionText.outlineWidth = 0.24f;

            _captionText.raycastTarget = false;

            _captionRoot.SetActive(false);
        }

        // =====================================
        // CLEANUP
        // =====================================

        private void OnDisable()
        {
            if (_inspectionSessionActive)
            {
                _inspectionSessionActive = false;
                _isInteracting = false;

                if (_fpsController != null)
                {
                    _fpsController.isInteracting = false;
                }

                InteractCameraSettings.Instance?.
                    NotInteracting();

                ToggleLantern(true);

                if (PlayerInteract.Instance != null)
                {
                    PlayerInteract.Instance.sendRaycast = true;
                }
            }

            SetPlayerLightActive(false);
            HideInspectCaption();
        }

        private void OnDestroy()
        {
            HideInspectCaption();
        }

        public void HoldInteract()
        {
        }

        public void UnHighlight()
        {
        }
    }
}
