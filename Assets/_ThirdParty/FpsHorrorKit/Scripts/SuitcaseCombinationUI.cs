using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FpsHorrorKit
{
    [DisallowMultipleComponent]
    public class SuitcaseCombinationUI : MonoBehaviour
    {
        private readonly int[] digits = new int[4];
        private readonly TextMeshProUGUI[] previousDigits = new TextMeshProUGUI[4];
        private readonly TextMeshProUGUI[] currentDigits = new TextMeshProUGUI[4];
        private readonly TextMeshProUGUI[] nextDigits = new TextMeshProUGUI[4];

        private GameObject uiRoot;
        private CanvasGroup canvasGroup;
        private TextMeshProUGUI feedbackText;
        private CombinationSuitcase activeSuitcase;
        private FpsController controller;

        public bool IsOpen => uiRoot != null && uiRoot.activeSelf;

        private void Awake() => BuildUi();

        private void Update()
        {
            if (uiRoot != null && uiRoot.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        public void Open(CombinationSuitcase suitcase)
        {
            BuildUi();
            if (IsOpen)
            {
                return;
            }

            activeSuitcase = suitcase;
            for (int index = 0; index < digits.Length; index++) digits[index] = 0;
            RefreshWheels();
            feedbackText.text = "Quay các vòng số rồi nhấn MỞ KHÓA";
            feedbackText.color = new Color(0.85f, 0.85f, 0.8f, 1f);
            uiRoot.SetActive(true);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            controller = FindAnyObjectByType<FpsController>();
            if (controller != null) controller.isInteracting = true;
            if (PlayerInteract.Instance != null) PlayerInteract.Instance.sendRaycast = false;
            InteractCameraSettings.Instance?.ShowCursor();
        }

        public void Close()
        {
            if (uiRoot == null) return;
            uiRoot.SetActive(false);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            activeSuitcase = null;
            if (controller != null) controller.isInteracting = false;
            if (PlayerInteract.Instance != null) PlayerInteract.Instance.sendRaycast = true;
            InteractCameraSettings.Instance?.HideCursor();
        }

        private void ChangeDigit(int index, int amount)
        {
            digits[index] = (digits[index] + amount + 10) % 10;
            RefreshWheels();
            feedbackText.text = string.Empty;
        }

        private void Submit()
        {
            if (activeSuitcase == null)
            {
                Close();
                return;
            }

            string code = string.Concat(digits[0], digits[1], digits[2], digits[3]);
            if (activeSuitcase.TryUnlock(code))
            {
                Close();
                return;
            }

            feedbackText.text = "MÃ SAI";
            feedbackText.color = new Color(1f, 0.34f, 0.25f, 1f);
        }

        private void RefreshWheels()
        {
            CacheWheelLabels();
            for (int index = 0; index < digits.Length; index++)
            {
                if (previousDigits[index] == null || currentDigits[index] == null || nextDigits[index] == null)
                {
                    continue;
                }

                previousDigits[index].text = ((digits[index] + 9) % 10).ToString();
                currentDigits[index].text = digits[index].ToString();
                nextDigits[index].text = ((digits[index] + 1) % 10).ToString();
            }
        }

        private void BuildUi()
        {
            if (uiRoot != null)
            {
                CacheWheelLabels();
                return;
            }

            uiRoot = new GameObject("Suitcase Combination UI", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            uiRoot.transform.SetParent(transform, false);
            Canvas canvas = uiRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            CanvasScaler scaler = uiRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGroup = uiRoot.GetComponent<CanvasGroup>();
            SetStretch(uiRoot.GetComponent<RectTransform>());

            CreateImage("Dimmer", uiRoot.transform, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.78f), true);
            RectTransform panel = CreateImage("Metal Combination Panel", uiRoot.transform, Vector2.zero,
                new Vector2(720f, 480f), new Color(0.20f, 0.21f, 0.22f, 1f), false);
            AddOutline(panel.gameObject, new Color(0.68f, 0.66f, 0.58f, 1f), new Vector2(6f, -6f));
            CreateText("Title", panel, new Vector2(0f, 184f), new Vector2(550f, 46f), "MÃ KHÓA", 36, new Color(0.93f, 0.9f, 0.78f, 1f));
            CreateText("Subtitle", panel, new Vector2(0f, 138f), new Vector2(520f, 28f), "QUAY 4 VÒNG SỐ", 18, new Color(0.65f, 0.66f, 0.64f, 1f));

            RectTransform slotFrame = CreateImage("Number Slot Frame", panel, new Vector2(0f, 10f),
                new Vector2(600f, 230f), new Color(0.07f, 0.075f, 0.08f, 1f), false);
            AddOutline(slotFrame.gameObject, new Color(0.52f, 0.50f, 0.46f, 1f), new Vector2(4f, -4f));

            for (int index = 0; index < 4; index++)
            {
                int wheelIndex = index;
                RectTransform wheel = CreateImage("Wheel " + (index + 1), slotFrame, new Vector2(-195f + (130f * index), 0f),
                    new Vector2(98f, 182f), new Color(0.72f, 0.70f, 0.64f, 1f), false);
                AddOutline(wheel.gameObject, new Color(0.12f, 0.12f, 0.12f, 1f), new Vector2(3f, -3f));
                CreateButton("Up " + index, wheel, new Vector2(0f, 70f), new Vector2(76f, 30f), "▲", () => ChangeDigit(wheelIndex, 1), new Color(0.34f, 0.35f, 0.35f, 1f));
                RectTransform window = CreateImage("Digit Window " + index, wheel, Vector2.zero, new Vector2(78f, 93f), new Color(0.055f, 0.055f, 0.058f, 1f), false);
                previousDigits[index] = CreateText("Previous " + index, window, new Vector2(0f, 31f), new Vector2(72f, 36f), "9", 31, new Color(0.47f, 0.47f, 0.46f, 1f));
                currentDigits[index] = CreateText("Current " + index, window, Vector2.zero, new Vector2(72f, 49f), "0", 52, new Color(0.96f, 0.94f, 0.86f, 1f));
                nextDigits[index] = CreateText("Next " + index, window, new Vector2(0f, -31f), new Vector2(72f, 36f), "1", 31, new Color(0.47f, 0.47f, 0.46f, 1f));
                CreateButton("Down " + index, wheel, new Vector2(0f, -70f), new Vector2(76f, 30f), "▼", () => ChangeDigit(wheelIndex, -1), new Color(0.34f, 0.35f, 0.35f, 1f));
            }

            feedbackText = CreateText("Feedback", panel, new Vector2(0f, -145f), new Vector2(560f, 30f), string.Empty, 20, new Color(0.85f, 0.85f, 0.8f, 1f));
            CreateButton("Unlock", panel, new Vector2(0f, -202f), new Vector2(260f, 52f), "MỞ KHÓA", Submit, new Color(0.16f, 0.42f, 0.25f, 1f));
            CreateButton("Close", panel, new Vector2(310f, 205f), new Vector2(48f, 38f), "X", Close, new Color(0.42f, 0.16f, 0.14f, 1f));
            uiRoot.SetActive(false);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        private void CacheWheelLabels()
        {
            if (uiRoot == null)
            {
                return;
            }

            for (int index = 0; index < digits.Length; index++)
            {
                if (previousDigits[index] != null && currentDigits[index] != null && nextDigits[index] != null)
                {
                    continue;
                }

                string basePath = "Metal Combination Panel/Number Slot Frame/Wheel " + (index + 1) +
                    "/Digit Window " + index + "/";
                previousDigits[index] = FindText(basePath + "Previous " + index);
                currentDigits[index] = FindText(basePath + "Current " + index);
                nextDigits[index] = FindText(basePath + "Next " + index);
            }
        }

        private TextMeshProUGUI FindText(string path)
        {
            Transform label = uiRoot.transform.Find(path);
            return label != null ? label.GetComponent<TextMeshProUGUI>() : null;
        }

        private static RectTransform CreateImage(string name, Transform parent, Vector2 position, Vector2 size, Color color, bool stretch)
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

        private static void CreateButton(string name, Transform parent, Vector2 position, Vector2 size, string label, UnityEngine.Events.UnityAction action, Color color)
        {
            RectTransform rect = CreateImage(name, parent, position, size, color, false);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<RawImage>();
            button.onClick.AddListener(action);
            CreateText("Label", rect, Vector2.zero, size, label, 23, Color.white);
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
