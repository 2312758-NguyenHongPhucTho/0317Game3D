using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FpsHorrorKit
{
    /// <summary>
    /// No-inventory trap escape interaction. E repeatedly fills a visible progress bar.
    /// Bear traps can temporarily aim the FPS camera at a configurable foot/hinge target.
    /// Keeps movement and PlayerInteract locked until the camera has recovered.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrapEscapeHandler0317 : MonoBehaviour
    {
        [Header("Player references")]
        [SerializeField] private FpsController fpsController;
        [SerializeField] private PlayerInteract playerInteract;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private PlayerHealth0317 playerHealth;
        [Tooltip("Optional additional movement/input scripts. Do NOT add this handler, PlayerHealth or PlayerBlinkEffect.")]
        [SerializeField] private Behaviour[] extraControlsToDisable;

        [Header("Camera")]
        [SerializeField, Min(0.01f)] private float lookDownSeconds = 0.45f;
        [SerializeField, Min(0.01f)] private float lookBackSeconds = 0.32f;
        [SerializeField, Min(0f)] private float successDisplaySeconds = 0.2f;
        [SerializeField, Min(1f)] private float teleportCancellationDistance = 2.0f;

        [Header("Optional UI font")]
        [SerializeField] private TMP_FontAsset vietnameseFont;
        [SerializeField] private Color progressColor = new Color(0.65f, 0.09f, 0.10f, 1f);

        private TrapEscapeZone0317 currentTrap;
        private int pressCount;
        private Vector3 lockedPosition;
        private bool recovering;
        private bool controlsStored;
        private bool originalFpsEnabled;
        private bool originalInteractEnabled;
        private bool[] originalExtraEnabled;
        private Quaternion cameraRotationBeforeTrap;
        private bool cameraPoseSaved;
        private Coroutine cameraRoutine;
        private Coroutine releaseRoutine;
        private float visualProgress;

        private Canvas uiCanvas;
        private TextMeshProUGUI titleLabel;
        private TextMeshProUGUI progressLabel;
        private TextMeshProUGUI hintLabel;
        private Image progressFill;

        public bool IsTrapped { get { return currentTrap != null; } }
        public bool IsReleasing { get { return recovering; } }

        private void Awake()
        {
            if (fpsController == null) fpsController = GetComponent<FpsController>();
            if (playerInteract == null) playerInteract = GetComponentInChildren<PlayerInteract>(true);
            if (playerInteract == null) playerInteract = FindFirstObjectByType<PlayerInteract>();
            if (gameplayCamera == null) gameplayCamera = GetComponentInChildren<Camera>(true);
            if (playerHealth == null) playerHealth = GetComponent<PlayerHealth0317>();
            CreateUI();
            ShowUI(false);
        }

        private void OnEnable()
        {
            if (playerHealth != null) playerHealth.onTrapDeath.AddListener(OnTrapDeath);
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.onTrapDeath.RemoveListener(OnTrapDeath);
            ForceRelease();
        }

        private void OnDestroy()
        {
            if (uiCanvas != null) Destroy(uiCanvas.gameObject);
        }

        private void Update()
        {
            if (currentTrap == null) return;

            // PlayerHealth0317 may teleport the character after a lethal hit.
            // Never pull the respawned player back to the trap.
            if (Vector3.Distance(transform.position, lockedPosition) > teleportCancellationDistance)
            {
                ForceRelease();
                return;
            }

            if (Keyboard.current != null &&
                Time.frameCount != catchFrame &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                pressCount = Mathf.Min(pressCount + 1, currentTrap.RequiredPressesToEscape);
                UpdateProgressLabels(false);

                if (pressCount >= currentTrap.RequiredPressesToEscape)
                {
                    CompleteEscape();
                    return; // currentTrap becomes null after success
                }
            }
            if (progressFill != null)
            {
                float goal = Mathf.Clamp01((float)pressCount / currentTrap.RequiredPressesToEscape);
                visualProgress = Mathf.MoveTowards(visualProgress, goal, Time.unscaledDeltaTime * 6f);
                progressFill.fillAmount = visualProgress;
            }
        }

        private int catchFrame;

        private void LateUpdate()
        {
            if (currentTrap == null || !currentTrap.LockPlayerInPlace) return;
            if (Vector3.Distance(transform.position, lockedPosition) > teleportCancellationDistance)
            {
                ForceRelease();
                return;
            }
            transform.position = lockedPosition;
        }

        public bool TryBeginTrap(TrapEscapeZone0317 trap)
        {
            if (trap == null || currentTrap != null || recovering || !isActiveAndEnabled) return false;
            if (playerHealth != null && (playerHealth.IsDeadFromTrap || !playerHealth.CanReceiveTrapDamage))
                return false;

            currentTrap = trap;
            pressCount = 0;
            visualProgress = 0f;
            lockedPosition = transform.position;
            catchFrame = Time.frameCount;
            SaveAndDisableControls();

            cameraPoseSaved = gameplayCamera != null;
            if (cameraPoseSaved)
                cameraRotationBeforeTrap = gameplayCamera.transform.localRotation;

            CreateUI();
            ShowUI(true);
            UpdateProgressLabels(false);
            if (progressFill != null) progressFill.fillAmount = 0f;

            if (trap.LookDownWhenTrapped && gameplayCamera != null)
            {
                Vector3 target = trap.CameraLookTarget != null
                    ? trap.CameraLookTarget.position
                    : trap.transform.position + Vector3.up * 0.12f;
                Vector3 direction = target - gameplayCamera.transform.position;
                if (direction.sqrMagnitude > 0.01f)
                {
                    // Avoid an undefined roll when the target is almost straight below the camera.
                    Vector3 directionNormal = direction.normalized;
                    Vector3 referenceUp = Mathf.Abs(Vector3.Dot(directionNormal, Vector3.up)) > 0.97f
                        ? transform.forward : Vector3.up;
                    Quaternion worldRotation = Quaternion.LookRotation(directionNormal, referenceUp);
                    Transform parent = gameplayCamera.transform.parent;
                    Quaternion targetLocalRotation = parent != null
                        ? Quaternion.Inverse(parent.rotation) * worldRotation
                        : worldRotation;
                    if (cameraRoutine != null) StopCoroutine(cameraRoutine);
                    cameraRoutine = StartCoroutine(RotateCameraLocal(targetLocalRotation, lookDownSeconds));
                }
            }
            return true;
        }

        private void CompleteEscape()
        {
            if (currentTrap == null || recovering) return;
            TrapEscapeZone0317 finished = currentTrap;
            currentTrap = null;
            recovering = true;
            if (cameraRoutine != null)
            {
                StopCoroutine(cameraRoutine);
                cameraRoutine = null;
            }
            visualProgress = 1f;
            if (progressFill != null) progressFill.fillAmount = 1f;
            UpdateProgressLabels(true);
            finished.NotifyEscapeCompleted();
            releaseRoutine = StartCoroutine(ReleaseRoutine());
        }

        private IEnumerator ReleaseRoutine()
        {
            if (successDisplaySeconds > 0f)
                yield return new WaitForSecondsRealtime(successDisplaySeconds);

            if (cameraPoseSaved && gameplayCamera != null)
                yield return RotateCameraLocal(cameraRotationBeforeTrap, lookBackSeconds);

            cameraPoseSaved = false;
            ShowUI(false);
            RestoreControls();
            recovering = false;
            releaseRoutine = null;
        }

        /// <summary>Called by trap death / group disable / scene logic. Safe without an active trap.</summary>
        public void ForceRelease()
        {
            TrapEscapeZone0317 oldTrap = currentTrap;
            currentTrap = null;
            recovering = false;
            pressCount = 0;

            if (cameraRoutine != null) { StopCoroutine(cameraRoutine); cameraRoutine = null; }
            if (releaseRoutine != null) { StopCoroutine(releaseRoutine); releaseRoutine = null; }
            if (cameraPoseSaved && gameplayCamera != null)
                gameplayCamera.transform.localRotation = cameraRotationBeforeTrap;
            cameraPoseSaved = false;
            ShowUI(false);
            RestoreControls();

            if (oldTrap != null)
                oldTrap.NotifyForcedRelease();
        }

        private void OnTrapDeath() { ForceRelease(); }

        private IEnumerator RotateCameraLocal(Quaternion destination, float duration)
        {
            if (gameplayCamera == null) yield break;
            Quaternion from = gameplayCamera.transform.localRotation;
            float elapsed = 0f;
            duration = Mathf.Max(0.01f, duration);
            while (elapsed < duration && gameplayCamera != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t); // smoothstep
                gameplayCamera.transform.localRotation = Quaternion.Slerp(from, destination, t);
                yield return null;
            }
            if (gameplayCamera != null)
                gameplayCamera.transform.localRotation = destination;
            cameraRoutine = null;
        }

        private void SaveAndDisableControls()
        {
            controlsStored = true;
            if (fpsController != null) { originalFpsEnabled = fpsController.enabled; fpsController.enabled = false; }
            if (playerInteract != null) { originalInteractEnabled = playerInteract.enabled; playerInteract.enabled = false; }
            int n = extraControlsToDisable == null ? 0 : extraControlsToDisable.Length;
            originalExtraEnabled = new bool[n];
            for (int i = 0; i < n; i++)
            {
                Behaviour b = extraControlsToDisable[i];
                if (b == null || b == this || b == fpsController || b == playerInteract) continue;
                originalExtraEnabled[i] = b.enabled;
                b.enabled = false;
            }
        }

        private void RestoreControls()
        {
            if (!controlsStored) return;
            controlsStored = false;
            if (fpsController != null) fpsController.enabled = originalFpsEnabled;
            if (playerInteract != null) playerInteract.enabled = originalInteractEnabled;
            int n = extraControlsToDisable == null ? 0 : extraControlsToDisable.Length;
            for (int i = 0; i < n; i++)
            {
                Behaviour b = extraControlsToDisable[i];
                if (b == null || b == this || b == fpsController || b == playerInteract) continue;
                if (originalExtraEnabled != null && i < originalExtraEnabled.Length)
                    b.enabled = originalExtraEnabled[i];
            }
        }

        private void ShowUI(bool show)
        {
            if (uiCanvas != null) uiCanvas.gameObject.SetActive(show);
        }

        private void UpdateProgressLabels(bool success)
        {
            if (uiCanvas == null) return;
            int total = currentTrap != null ? currentTrap.RequiredPressesToEscape : Mathf.Max(1, pressCount);
            int percentage = success ? 100 : Mathf.RoundToInt(100f * pressCount / total);
            if (titleLabel != null) titleLabel.text = success ? "ĐÃ GỠ BẪY" : "BỊ MẮC BẪY!";
            if (progressLabel != null) progressLabel.text = success ? "Đã thoát!" : "Gỡ bẫy... " + percentage + "%";
            if (hintLabel != null) hintLabel.text = success ? "" : "NHẤN [E] LIÊN TỤC ĐỂ THOÁT";
        }

        private void CreateUI()
        {
            if (uiCanvas != null) return;
            GameObject root = new GameObject("0317_TrapEscapeUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.transform.SetParent(transform, false);
            uiCanvas = root.GetComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.sortingOrder = 4000; // HP=1500; cinematic subtitles=8000
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject panel = CreateRect("TrapPanel", root.transform, new Vector2(650f, 172f));
            RectTransform panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.18f);
            panelRect.anchorMax = new Vector2(0.5f, 0.18f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            Image background = panel.AddComponent<Image>();
            background.color = new Color(0.045f, 0.025f, 0.025f, 0.84f);
            background.raycastTarget = false;

            titleLabel = MakeText("Title", panel.transform, new Vector2(580f, 42f), new Vector2(0f, 51f), 33f, new Color(1f, 0.54f, 0.47f, 1f));
            progressLabel = MakeText("ProgressText", panel.transform, new Vector2(580f, 32f), new Vector2(0f, 14f), 26f, Color.white);
            hintLabel = MakeText("Hint", panel.transform, new Vector2(610f, 34f), new Vector2(0f, -56f), 22f, new Color(0.95f, 0.89f, 0.8f, 1f));

            GameObject barBack = CreateRect("BarBackground", panel.transform, new Vector2(555f, 19f));
            ((RectTransform)barBack.transform).anchoredPosition = new Vector2(0f, -23f);
            Image barBgImage = barBack.AddComponent<Image>();
            barBgImage.color = new Color(0.18f, 0.16f, 0.16f, 1f);
            barBgImage.raycastTarget = false;

            GameObject fillObj = CreateRect("BarFill", barBack.transform, Vector2.zero);
            RectTransform fillRect = (RectTransform)fillObj.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            progressFill = fillObj.AddComponent<Image>();
            progressFill.color = progressColor;
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0;
            progressFill.fillAmount = 0f;
            progressFill.raycastTarget = false;
            root.SetActive(false);
        }

        private static GameObject CreateRect(string objectName, Transform parent, Vector2 size)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            return go;
        }

        private TextMeshProUGUI MakeText(string objectName, Transform parent, Vector2 size, Vector2 offset, float fontSize, Color color)
        {
            GameObject go = CreateRect(objectName, parent, size);
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = fontSize;
            label.color = color;
            label.raycastTarget = false;
            label.enableWordWrapping = false;
            if (vietnameseFont != null) label.font = vietnameseFont;
            ((RectTransform)go.transform).anchoredPosition = offset;
            return label;
        }
    }
}
