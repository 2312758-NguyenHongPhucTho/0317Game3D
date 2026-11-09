using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FpsHorrorKit
{
    /// <summary>
    /// Self-contained 100 HP HUD. Add next to PlayerHealth0317 on Player.
    /// Creates an overlay Canvas at runtime, with no prefab or inventory required.
    /// Sorting order stays below LamAttackCutscene's blackout canvas (8000).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HealthBar0317 : MonoBehaviour
    {
        [SerializeField] private PlayerHealth0317 health;
        [SerializeField] private TMP_FontAsset optionalFont;
        [SerializeField] private Vector2 topLeftOffset = new Vector2(25f, -25f);
        [SerializeField] private Vector2 panelSize = new Vector2(330f, 74f);

        private GameObject uiRoot;
        private Image fill;
        private TextMeshProUGUI hpText;
        private GameObject deathOverlay;
        private Coroutine flashRoutine;
        private Color normalFillColor = new Color(0.75f, 0.09f, 0.11f, 1f);
        private Color lowFillColor = new Color(0.95f, 0.20f, 0.12f, 1f);

        private void Awake()
        {
            if (health == null) health = GetComponent<PlayerHealth0317>();
            if (health == null) health = GetComponentInParent<PlayerHealth0317>();
            BuildHUD();
        }

        private void Start() { Refresh(); }

        private void OnEnable()
        {
            if (health != null)
            {
                health.HealthChanged += HandleHealthChanged;
                health.TrapDamaged += HandleTrapDamaged;
            }
            Refresh();
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.HealthChanged -= HandleHealthChanged;
                health.TrapDamaged -= HandleTrapDamaged;
            }
        }

        private void OnDestroy()
        {
            if (uiRoot != null) Destroy(uiRoot);
        }

        public void ShowHUD() { if (uiRoot != null) uiRoot.SetActive(true); }
        public void HideHUD() { if (uiRoot != null) uiRoot.SetActive(false); }

        private void HandleHealthChanged(int hp, int maximum) { Refresh(); }

        private void HandleTrapDamaged(int damage)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashBar());
        }

        private IEnumerator FlashBar()
        {
            if (fill == null) yield break;
            fill.color = Color.white;
            yield return new WaitForSecondsRealtime(0.14f);
            Refresh();
            flashRoutine = null;
        }

        private void Refresh()
        {
            if (health == null || fill == null || hpText == null) return;
            // Anchor-based horizontal bar works even without a Sprite asset.
            fill.rectTransform.anchorMax = new Vector2(health.HealthNormalized, 1f);
            fill.color = health.HealthNormalized <= 0.3f ? lowFillColor : normalFillColor;
            hpText.text = "HP   " + health.CurrentHealth + " / " + health.MaxHealth;
            if (deathOverlay != null) deathOverlay.SetActive(health.IsDeadFromTrap);
        }

        private void BuildHUD()
        {
            uiRoot = new GameObject("0317_Health_HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            uiRoot.transform.SetParent(transform, false);
            Canvas canvas = uiRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1500; // Below LamAttackCutscene (8000).
            CanvasScaler scaler = uiRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            RectTransform root = uiRoot.GetComponent<RectTransform>();

            RectTransform panel = MakeImage("HP_Panel", root,
                new Color(0.035f, 0.023f, 0.025f, 0.82f)).rectTransform;
            panel.anchorMin = new Vector2(0f, 1f);
            panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(0f, 1f);
            panel.anchoredPosition = topLeftOffset;
            panel.sizeDelta = panelSize;

            Image track = MakeImage("Bar_Track", panel, new Color(0.18f, 0.15f, 0.15f, 1f));
            RectTransform trackRect = track.rectTransform;
            trackRect.anchorMin = new Vector2(0f, 0.16f);
            trackRect.anchorMax = new Vector2(1f, 0.52f);
            trackRect.offsetMin = new Vector2(13f, 0f);
            trackRect.offsetMax = new Vector2(-13f, 0f);

            fill = MakeImage("Bar_Fill", trackRect, normalFillColor);
            Stretch(fill.rectTransform);
            fill.type = Image.Type.Simple;

            GameObject labelGo = new GameObject("HP_Count", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(panel, false);
            RectTransform lr = labelGo.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0f, 0.55f);
            lr.anchorMax = new Vector2(1f, 0.98f);
            lr.offsetMin = new Vector2(12f, 0f);
            lr.offsetMax = new Vector2(-12f, 0f);
            hpText = labelGo.GetComponent<TextMeshProUGUI>();
            if (optionalFont != null) hpText.font = optionalFont;
            hpText.fontSize = 23f;
            hpText.alignment = TextAlignmentOptions.MidlineLeft;
            hpText.color = Color.white;
            hpText.raycastTarget = false;

            // Black-out only on trap death. Cinematic death uses its existing own UI.
            deathOverlay = new GameObject("TrapDeathOverlay", typeof(RectTransform), typeof(Image));
            deathOverlay.transform.SetParent(root, false);
            RectTransform deathRect = deathOverlay.GetComponent<RectTransform>();
            Stretch(deathRect);
            Image black = deathOverlay.GetComponent<Image>();
            black.color = new Color(0.02f, 0.005f, 0.005f, 0.92f);
            black.raycastTarget = false;
            GameObject textGo = new GameObject("DeathText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(deathRect, false);
            RectTransform tr = textGo.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.1f, 0.43f);
            tr.anchorMax = new Vector2(0.9f, 0.57f);
            tr.offsetMin = tr.offsetMax = Vector2.zero;
            TextMeshProUGUI text = textGo.GetComponent<TextMeshProUGUI>();
            if (optionalFont != null) text.font = optionalFont;
            text.text = "BẠN ĐÃ GỤC NGÃ\n03:17...";
            text.fontSize = 44f;
            text.color = new Color(0.8f, 0.17f, 0.17f);
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            deathOverlay.SetActive(false);
        }

        private static Image MakeImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
