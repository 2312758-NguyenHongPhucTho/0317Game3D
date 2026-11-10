using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

namespace FpsHorrorKit
{
    /// <summary>
    /// Optional pre-trigger disarm interaction for an existing 03:17 trap.
    /// Put this component on a SEPARATE child with a small BoxCollider (Is Trigger).
    /// Do not put it on Trap_Trigger or replace TrapEscapeHandler0317.
    /// The existing PlayerInteract system sends E presses to Interact().
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TrapDisarmInteract0317 : MonoBehaviour, IInteractable
    {
        [Header("Existing trap (assign ONE)")]
        [SerializeField] private TrapEscapeZone0317 bearTrap;
        [SerializeField] private DamageTrap0317 damageTrap;

        [Header("Disarm interaction")]
        [SerializeField, Min(.1f)] private float holdSeconds = 3f;
        [SerializeField] private TMP_FontAsset vietnameseFont;

        [Header("Optional feedback")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip disarmedClip;
        [SerializeField] private UnityEvent onDisarmed = new UnityEvent();

        private BoxCollider interactionCollider;
        private float holdElapsed;
        private bool holding;
        private bool disarmed;
        private TrapEscapeHandler0317 escapeHandler;
        private Canvas progressCanvas;
        private TextMeshProUGUI progressLabel;
        private Image progressFill;

        public bool IsDisarmed => disarmed;

        private void Reset()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null) box.isTrigger = true;
        }

        private void Awake()
        {
            interactionCollider = GetComponent<BoxCollider>();
            escapeHandler = FindFirstObjectByType<TrapEscapeHandler0317>();
            if (interactionCollider != null && !interactionCollider.isTrigger)
                Debug.LogWarning("[03:17] TrapDisarmInteract0317: set the separate interaction BoxCollider to Is Trigger.", this);
            if ((bearTrap == null) == (damageTrap == null))
                Debug.LogWarning("[03:17] TrapDisarmInteract0317: assign exactly one existing trap reference.", this);
            DisableInteractionIfTargetDisarmed();
        }

        private void Update()
        {
            DisableInteractionIfTargetDisarmed();
            UpdateHold();
        }

        public void Interact()
        {
            if (disarmed || holding || !isActiveAndEnabled || TargetIsDisarmed()) return;
            if ((bearTrap == null) == (damageTrap == null)) return;
            if (escapeHandler == null) escapeHandler = FindFirstObjectByType<TrapEscapeHandler0317>();
            if (escapeHandler != null && (escapeHandler.IsTrapped || escapeHandler.IsReleasing)) return;

            if (Keyboard.current == null || !Keyboard.current.eKey.isPressed) return;
            holding = true;
            holdElapsed = 0f;
            EnsureProgressUI();
            ShowProgress(true);
            UpdateProgressUI();
        }

        private void UpdateHold()
        {
            if (!holding) return;
            if (Time.timeScale <= 0f || disarmed || TargetIsDisarmed() ||
                (escapeHandler != null && (escapeHandler.IsTrapped || escapeHandler.IsReleasing)) ||
                Keyboard.current == null || !Keyboard.current.eKey.isPressed || !IsStillTargeted())
            {
                CancelHold();
                return;
            }

            holdElapsed += Time.deltaTime;
            UpdateProgressUI();
            if (holdElapsed < Mathf.Max(.1f, holdSeconds)) return;

            if (bearTrap != null)
                bearTrap.DisarmTrap();
            else if (damageTrap != null)
                damageTrap.SetTrapActive(false);

            DisableInteraction();

            if (audioSource != null && disarmedClip != null)
                audioSource.PlayOneShot(disarmedClip);
            onDisarmed?.Invoke();
            Debug.Log("[03:17] Trap safely disarmed: " + name, this);
        }

        public void Highlight()
        {
            if (PlayerInteract.Instance == null) return;
            if (disarmed || TargetIsDisarmed()) return;
            PlayerInteract.Instance.ChangeInteractText("Giữ [E] trong 3 giây để gỡ bẫy");
        }

        public void HoldInteract() { }
        public void UnHighlight() { CancelHold(); }

        /// <summary>
        /// Call this when the game intentionally rearms the corresponding trap.
        /// It only resets the interaction; rearm the actual trap separately.
        /// </summary>
        public void ResetDisarmInteraction()
        {
            disarmed = false;
            CancelHold();
            if (interactionCollider == null)
                interactionCollider = GetComponent<BoxCollider>();
            if (interactionCollider != null)
                interactionCollider.enabled = true;
        }

        private bool TargetIsDisarmed()
        {
            if (bearTrap != null)
            {
                Collider trapCollider = bearTrap.GetComponent<Collider>();
                return !bearTrap.isActiveAndEnabled || (trapCollider != null && !trapCollider.enabled);
            }
            return damageTrap != null && !damageTrap.IsTrapActive;
        }

        private void DisableInteractionIfTargetDisarmed()
        {
            if (!disarmed && TargetIsDisarmed()) DisableInteraction();
        }

        private void DisableInteraction()
        {
            disarmed = true;
            CancelHold();
            if (interactionCollider != null) interactionCollider.enabled = false;
        }

        private bool IsStillTargeted()
        {
            Camera camera = Camera.main;
            PlayerInteract interact = PlayerInteract.Instance;
            if (camera == null || interact == null || interactionCollider == null) return false;
            Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width * .5f, Screen.height * .5f));
            return Physics.Raycast(ray, out RaycastHit hit, interact.interactRange) && hit.collider == interactionCollider;
        }

        private void CancelHold()
        {
            holding = false;
            holdElapsed = 0f;
            UpdateProgressUI();
            ShowProgress(false);
        }

        private void EnsureProgressUI()
        {
            if (progressCanvas != null) return;
            GameObject root = new GameObject("0317_TrapDisarmUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.transform.SetParent(transform, false);
            progressCanvas = root.GetComponent<Canvas>();
            progressCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            progressCanvas.sortingOrder = 3500;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(root.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = labelRect.anchorMax = new Vector2(.5f, .30f);
            labelRect.pivot = new Vector2(.5f, .5f);
            labelRect.sizeDelta = new Vector2(520f, 54f);
            progressLabel = labelObject.GetComponent<TextMeshProUGUI>();
            if (vietnameseFont != null) progressLabel.font = vietnameseFont;
            progressLabel.fontSize = 27f;
            progressLabel.alignment = TextAlignmentOptions.Center;
            progressLabel.color = Color.white;
            progressLabel.raycastTarget = false;

            GameObject barObject = new GameObject("Progress", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            barObject.transform.SetParent(root.transform, false);
            RectTransform barRect = barObject.GetComponent<RectTransform>();
            barRect.anchorMin = barRect.anchorMax = new Vector2(.5f, .26f);
            barRect.pivot = new Vector2(.5f, .5f);
            barRect.sizeDelta = new Vector2(420f, 18f);
            progressFill = barObject.GetComponent<Image>();
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.color = new Color(.85f, .12f, .08f, 1f);
            progressFill.raycastTarget = false;
            root.SetActive(false);
        }

        private void UpdateProgressUI()
        {
            float progress = Mathf.Clamp01(holdElapsed / Mathf.Max(.1f, holdSeconds));
            if (progressLabel != null) progressLabel.text = "GỠ BẪY\nGIỮ [E] – " + Mathf.RoundToInt(progress * 100f) + "%";
            if (progressFill != null) progressFill.fillAmount = progress;
        }

        private void ShowProgress(bool show)
        {
            if (progressCanvas != null) progressCanvas.gameObject.SetActive(show);
        }

        private void OnDestroy()
        {
            if (progressCanvas != null) Destroy(progressCanvas.gameObject);
        }

        private void OnDisable()
        {
            CancelHold();
        }
    }
}
