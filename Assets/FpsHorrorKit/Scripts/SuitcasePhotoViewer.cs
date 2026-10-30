using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FpsHorrorKit
{
    /// <summary>Lets the player inspect one photo from the open suitcase.</summary>
    [DisallowMultipleComponent]
    public class SuitcasePhotoViewer : MonoBehaviour, IInteractable
    {
        [SerializeField] private string photoTitle = "Tranh";
        [Tooltip("Kéo texture ảnh vào đây khi bạn đã có ảnh thật.")]
        [SerializeField] private Texture photoTexture;

        private GameObject viewerRoot;
        private RawImage preview;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI placeholderText;
        private FpsController controller;
        private CombinationSuitcase suitcase;

        private void Awake()
        {
            suitcase = GetComponentInParent<CombinationSuitcase>();
            BuildViewer();
        }

        private void Update()
        {
            if (IsViewing && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape)))
            {
                Close();
            }
        }

        public void Interact()
        {
            if (CanViewPhoto)
            {
                Open();
            }
        }

        public void HoldInteract() { }

        public void Highlight()
        {
            PlayerInteract.Instance?.ChangeInteractText(CanViewPhoto
                ? "Nhấn [E] để xem " + photoTitle
                : "Phải mở khóa vali trước");
        }

        public void UnHighlight() { }

        private bool IsViewing => viewerRoot != null && viewerRoot.activeSelf;
        private bool CanViewPhoto => suitcase == null || suitcase.IsOpened;

        private void Open()
        {
            BuildViewer();
            if (IsViewing)
            {
                return;
            }

            titleText.text = photoTitle;
            preview.texture = photoTexture != null ? photoTexture : Texture2D.whiteTexture;
            preview.color = photoTexture != null ? Color.white : new Color(0.95f, 0.88f, 0.67f, 1f);
            placeholderText.gameObject.SetActive(photoTexture == null);
            viewerRoot.SetActive(true);

            controller = FindAnyObjectByType<FpsController>();
            if (controller != null) controller.isInteracting = true;
            if (PlayerInteract.Instance != null) PlayerInteract.Instance.sendRaycast = false;
            InteractCameraSettings.Instance?.ShowCursor();
        }

        private void Close()
        {
            if (viewerRoot == null)
            {
                return;
            }

            viewerRoot.SetActive(false);
            if (controller != null) controller.isInteracting = false;
            if (PlayerInteract.Instance != null) PlayerInteract.Instance.sendRaycast = true;
            InteractCameraSettings.Instance?.HideCursor();
        }

        private void BuildViewer()
        {
            if (viewerRoot != null)
            {
                CacheReferences();
                return;
            }

            viewerRoot = new GameObject(photoTitle + " Viewer", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            Canvas canvas = viewerRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32100;
            CanvasScaler scaler = viewerRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            SetStretch(viewerRoot.GetComponent<RectTransform>());

            CreateRawImage("Dimmer", viewerRoot.transform, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.86f), true);
            RectTransform frame = CreateRawImage("Photo Frame", viewerRoot.transform, new Vector2(0f, 20f),
                new Vector2(820f, 650f), new Color(0.12f, 0.085f, 0.045f, 1f), false);
            AddOutline(frame.gameObject, new Color(0.75f, 0.60f, 0.28f, 1f), new Vector2(6f, -6f));

            RectTransform paper = CreateRawImage("Photo", frame, Vector2.zero, new Vector2(730f, 540f), Color.white, false);
            preview = paper.GetComponent<RawImage>();
            preview.texture = Texture2D.whiteTexture;
            titleText = CreateText("Title", frame, new Vector2(0f, 297f), new Vector2(650f, 40f), photoTitle, 30, new Color(0.95f, 0.88f, 0.67f, 1f));
            placeholderText = CreateText("Placeholder", paper, Vector2.zero, new Vector2(600f, 80f), "THÊM ẢNH VÀO ĐÂY", 36, new Color(0.23f, 0.16f, 0.08f, 1f));
            CreateText("Close Hint", viewerRoot.transform, new Vector2(0f, -365f), new Vector2(600f, 32f), "Nhấn Q để bỏ tranh xuống", 22, Color.white);
            viewerRoot.SetActive(false);
        }

        private void CacheReferences()
        {
            if (viewerRoot == null)
            {
                return;
            }

            Transform frame = viewerRoot.transform.Find("Photo Frame");
            if (frame == null)
            {
                return;
            }

            Transform photo = frame.Find("Photo");
            preview = photo != null ? photo.GetComponent<RawImage>() : null;
            titleText = frame.Find("Title")?.GetComponent<TextMeshProUGUI>();
            placeholderText = photo != null ? photo.Find("Placeholder")?.GetComponent<TextMeshProUGUI>() : null;
        }

        private static RectTransform CreateRawImage(string name, Transform parent, Vector2 position, Vector2 size, Color color, bool stretch)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            if (stretch) SetStretch(rect);
            else { rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.anchoredPosition = position; rect.sizeDelta = size; }
            RawImage image = gameObject.GetComponent<RawImage>();
            image.texture = Texture2D.whiteTexture;
            image.color = color;
            return rect;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 position, Vector2 size, string content, float fontSize, Color color)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            TextMeshProUGUI text = gameObject.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static void AddOutline(GameObject gameObject, Color color, Vector2 distance)
        {
            Outline outline = gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
        }

        private static void SetStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
