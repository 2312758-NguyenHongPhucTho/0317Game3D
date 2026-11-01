using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Intro menu that previews Room_8 from a fixed menu camera. New Game hands
/// control back to the existing gameplay player.
/// </summary>
public sealed class RoomEightMainMenu : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private GameObject gameplayPlayer;
    [SerializeField] private Camera menuCamera;
    [SerializeField] private TMP_FontAsset titleFont;

    [Header("Presentation")]
    private GameObject menuCanvas;
    private bool isInMenu;
    private Behaviour[] playerBehaviours;
    private bool[] playerBehaviourStates;
    private CharacterController playerCharacterController;
    private bool playerCharacterControllerState;
    private float lastUnscaledTime;
    private bool runtimeClockStarted;
    private readonly List<MenuButtonVisual> menuButtons = new List<MenuButtonVisual>();
    private RectTransform newGameRect;
    private GameObject settingsOverlay;
    private bool settingsOpen;
    private FpsHorrorKit.FpsController playerController;
    private float defaultMouseSensitivity = 1f;
    private Slider masterVolumeSlider;
    private Slider mouseSensitivitySlider;
    private TextMeshProUGUI masterVolumeValue;
    private TextMeshProUGUI mouseSensitivityValue;

    private const string MasterVolumePreference = "Room8.MasterVolume";
    private const string MouseSensitivityPreference = "Room8.MouseSensitivity";

    private sealed class MenuButtonVisual
    {
        public RectTransform rect;
        public Image image;
    }

    private void Awake()
    {
        CacheGameplayControls();
        LoadSavedSettings();
        BuildMenu();
        OpenMenu();
    }

    private void Update()
    {
        // Supports Unity's "Enter Play Mode Options" when scene reload is off.
        // A new game session resets Time, while the component instance can persist.
        if (!runtimeClockStarted)
        {
            lastUnscaledTime = Time.unscaledTime;
            runtimeClockStarted = true;
            return;
        }

        if (Time.unscaledTime < lastUnscaledTime)
        {
            if (menuCanvas != null)
                Destroy(menuCanvas);
            menuCanvas = null;
            BuildMenu();
            OpenMenu();
        }

        lastUnscaledTime = Time.unscaledTime;

        if (isInMenu)
            UpdateMenuMouseInput();
    }

    public void StartNewGame()
    {
        if (!isInMenu)
            return;

        isInMenu = false;
        if (menuCanvas != null)
            menuCanvas.SetActive(false);
        if (menuCamera != null)
            menuCamera.enabled = false;
        SetGameplayControls(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OpenSettings()
    {
        if (!isInMenu)
            return;

        settingsOpen = true;
        if (settingsOverlay == null)
            BuildSettingsOverlay();
        else
            settingsOverlay.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsOpen = false;
        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);
    }

    private void OpenMenu()
    {
        isInMenu = true;
        SetGameplayControls(false);
        if (menuCamera != null)
            menuCamera.enabled = true;
        if (menuCanvas != null)
            menuCanvas.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void UpdateMenuMouseInput()
    {
        // Keep the pointer available while the menu is visible, even if an
        // existing gameplay script tries to lock it on startup.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (settingsOpen)
            return;

        Vector2 pointerPosition = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : (Vector2)Input.mousePosition;

        bool newGameHovered = false;
        foreach (MenuButtonVisual visual in menuButtons)
        {
            bool hovered = RectTransformUtility.RectangleContainsScreenPoint(visual.rect, pointerPosition, null);
            visual.image.color = hovered
                ? new Color(0.86f, 0.045f, 0.025f, 0.98f)
                : new Color(0.14f, 0.02f, 0.015f, 0.10f);

            if (visual.rect == newGameRect)
                newGameHovered = hovered;
        }

        if (newGameHovered && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            StartNewGame();
    }

    private void CacheGameplayControls()
    {
        if (gameplayPlayer == null)
            return;

        // Keep the Player GameObject active: several existing interaction scripts
        // locate it by tag in Start(). Only movement/input is disabled for the menu.
        playerBehaviours = gameplayPlayer.GetComponents<Behaviour>();
        playerBehaviourStates = new bool[playerBehaviours.Length];
        for (int i = 0; i < playerBehaviours.Length; i++)
            playerBehaviourStates[i] = playerBehaviours[i].enabled;

        playerCharacterController = gameplayPlayer.GetComponent<CharacterController>();
        if (playerCharacterController != null)
            playerCharacterControllerState = playerCharacterController.enabled;

        playerController = gameplayPlayer.GetComponent<FpsHorrorKit.FpsController>();
        if (playerController != null)
            defaultMouseSensitivity = playerController.rotationSpeed;
    }

    private void LoadSavedSettings()
    {
        float volume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumePreference, AudioListener.volume));
        float sensitivity = Mathf.Clamp(
            PlayerPrefs.GetFloat(MouseSensitivityPreference, defaultMouseSensitivity),
            0.2f,
            3f);

        AudioListener.volume = volume;
        if (playerController != null)
            playerController.rotationSpeed = sensitivity;
    }

    private void SetMasterVolume(float value)
    {
        float volume = Mathf.Clamp01(value);
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(MasterVolumePreference, volume);
        PlayerPrefs.Save();

        if (masterVolumeValue != null)
            masterVolumeValue.text = $"{Mathf.RoundToInt(volume * 100f)}%";
    }

    private void SetMouseSensitivity(float value)
    {
        float sensitivity = Mathf.Clamp(value, 0.2f, 3f);
        if (playerController != null)
            playerController.rotationSpeed = sensitivity;

        PlayerPrefs.SetFloat(MouseSensitivityPreference, sensitivity);
        PlayerPrefs.Save();

        if (mouseSensitivityValue != null)
            mouseSensitivityValue.text = $"{sensitivity:0.0}x";
    }

    private void SetGameplayControls(bool enabled)
    {
        if (playerBehaviours != null)
        {
            for (int i = 0; i < playerBehaviours.Length; i++)
                playerBehaviours[i].enabled = enabled && playerBehaviourStates[i];
        }

        if (playerCharacterController != null)
            playerCharacterController.enabled = enabled && playerCharacterControllerState;
    }

    private void BuildMenu()
    {
        menuCanvas = new GameObject("Room8_MainMenu_UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = menuCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = menuCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRect = menuCanvas.GetComponent<RectTransform>();

        Image veil = CreateImage("Atmosphere_Veil", canvasRect, new Color(0.035f, 0.008f, 0.005f, 0.42f));
        Stretch(veil.rectTransform, Vector2.zero, Vector2.zero);

        Image panel = CreateImage("Left_Menu_Panel", canvasRect, new Color(0.015f, 0.006f, 0.004f, 0.78f));
        panel.rectTransform.anchorMin = new Vector2(0f, 0f);
        panel.rectTransform.anchorMax = new Vector2(0f, 1f);
        panel.rectTransform.pivot = new Vector2(0f, 0.5f);
        panel.rectTransform.sizeDelta = new Vector2(620f, 0f);
        panel.rectTransform.anchoredPosition = Vector2.zero;

        Image redRule = CreateImage("Blood_Rule", canvasRect, new Color(0.78f, 0.035f, 0.025f, 0.95f));
        redRule.rectTransform.anchorMin = new Vector2(0f, 1f);
        redRule.rectTransform.anchorMax = new Vector2(0f, 1f);
        redRule.rectTransform.pivot = new Vector2(0f, 1f);
        redRule.rectTransform.sizeDelta = new Vector2(10f, 820f);
        redRule.rectTransform.anchoredPosition = new Vector2(54f, -120f);

        TextMeshProUGUI title = CreateText("Title", canvasRect, "03:17", 96f, FontStyles.Bold, new Color(0.95f, 0.06f, 0.035f));
        title.font = titleFont != null
            ? titleFont
            : Resources.Load<TMP_FontAsset>("Fonts & Materials/BRADHITC SDF");
        title.rectTransform.anchorMin = new Vector2(0f, 1f);
        title.rectTransform.anchorMax = new Vector2(0f, 1f);
        title.rectTransform.pivot = new Vector2(0f, 1f);
        title.rectTransform.sizeDelta = new Vector2(500f, 120f);
        title.rectTransform.anchoredPosition = new Vector2(84f, -120f);
        title.enableWordWrapping = false;
        title.characterSpacing = 7f;

        CreateButton(canvasRect, "Continue_Button", "TIẾP TỤC", 300f, null);
        Button newGameButton = CreateButton(canvasRect, "NewGame_Button", "TRÒ CHƠI MỚI", 384f, StartNewGame);
        newGameRect = newGameButton.GetComponent<RectTransform>();
        CreateButton(canvasRect, "Settings_Button", "CÀI ĐẶT", 468f, OpenSettings);
        CreateButton(canvasRect, "Credits_Button", "ĐỘI NGŨ", 552f, null);
        CreateButton(canvasRect, "Quit_Button", "THOÁT", 636f, null);

        TextMeshProUGUI tip = CreateText("Tip", canvasRect, "Một căn phòng bị lãng quên đang chờ bạn.", 17f, FontStyles.Italic, new Color(0.82f, 0.72f, 0.66f, 0.82f));
        tip.rectTransform.anchorMin = new Vector2(0f, 0f);
        tip.rectTransform.anchorMax = new Vector2(0f, 0f);
        tip.rectTransform.pivot = new Vector2(0f, 0f);
        tip.rectTransform.sizeDelta = new Vector2(440f, 44f);
        tip.rectTransform.anchoredPosition = new Vector2(88f, 72f);

        TextMeshProUGUI footer = CreateText("Footer", canvasRect, "NHẤN TRÒ CHƠI MỚI ĐỂ BƯỚC VÀO", 14f, FontStyles.Bold, new Color(0.95f, 0.18f, 0.12f, 0.9f));
        footer.rectTransform.anchorMin = new Vector2(0f, 0f);
        footer.rectTransform.anchorMax = new Vector2(0f, 0f);
        footer.rectTransform.pivot = new Vector2(0f, 0f);
        footer.rectTransform.sizeDelta = new Vector2(460f, 30f);
        footer.rectTransform.anchoredPosition = new Vector2(88f, 40f);
        footer.characterSpacing = 1.2f;
    }

    private Button CreateButton(Transform parent, string objectName, string label, float topOffset, UnityEngine.Events.UnityAction onClick)
    {
        GameObject root = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(430f, 62f);
        rect.anchoredPosition = new Vector2(84f, -topOffset);

        Image image = root.GetComponent<Image>();
        image.color = new Color(0.14f, 0.02f, 0.015f, 0.10f);

        Button button = root.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.45f, 0.38f, 1f);
        colors.pressedColor = new Color(0.5f, 0.02f, 0.01f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        if (onClick != null)
            button.onClick.AddListener(onClick);

        menuButtons.Add(new MenuButtonVisual { rect = rect, image = image });

        TextMeshProUGUI text = CreateText("Label", rect, label, 27f, FontStyles.Bold, Color.white);
        Stretch(text.rectTransform, new Vector2(28f, 0f), new Vector2(-20f, 0f));
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.characterSpacing = 1.1f;
        return button;
    }

    private void BuildSettingsOverlay()
    {
        settingsOverlay = new GameObject("Settings_Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        settingsOverlay.transform.SetParent(menuCanvas.transform, false);
        RectTransform overlayRect = settingsOverlay.GetComponent<RectTransform>();
        Stretch(overlayRect, Vector2.zero, Vector2.zero);
        settingsOverlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.74f);

        Image card = CreateImage("Settings_Card", overlayRect, new Color(0.028f, 0.009f, 0.008f, 0.97f));
        RectTransform cardRect = card.rectTransform;
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(780f, 560f);
        cardRect.anchoredPosition = Vector2.zero;

        Image topRule = CreateImage("Settings_Red_Rule", cardRect, new Color(0.85f, 0.045f, 0.025f, 1f));
        topRule.rectTransform.anchorMin = new Vector2(0f, 1f);
        topRule.rectTransform.anchorMax = new Vector2(1f, 1f);
        topRule.rectTransform.pivot = new Vector2(0.5f, 1f);
        topRule.rectTransform.sizeDelta = new Vector2(0f, 8f);
        topRule.rectTransform.anchoredPosition = Vector2.zero;

        TextMeshProUGUI title = CreateText("Settings_Title", cardRect, "CÀI ĐẶT", 48f, FontStyles.Bold, new Color(0.96f, 0.08f, 0.04f));
        title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        title.rectTransform.pivot = new Vector2(0.5f, 1f);
        title.rectTransform.sizeDelta = new Vector2(680f, 66f);
        title.rectTransform.anchoredPosition = new Vector2(0f, -58f);
        title.alignment = TextAlignmentOptions.Center;
        title.characterSpacing = 2.5f;

        TextMeshProUGUI subtitle = CreateText("Settings_Subtitle", cardRect, "Điều chỉnh trải nghiệm của bạn", 18f, FontStyles.Italic, new Color(0.84f, 0.75f, 0.7f, 0.88f));
        subtitle.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        subtitle.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        subtitle.rectTransform.pivot = new Vector2(0.5f, 1f);
        subtitle.rectTransform.sizeDelta = new Vector2(680f, 32f);
        subtitle.rectTransform.anchoredPosition = new Vector2(0f, -120f);
        subtitle.alignment = TextAlignmentOptions.Center;

        CreateSettingsSection(cardRect, "ÂM LƯỢNG TỔNG", "Điều chỉnh toàn bộ âm thanh trong game", -210f,
            0f, 1f, AudioListener.volume, SetMasterVolume, out masterVolumeSlider, out masterVolumeValue, true);
        CreateSettingsSection(cardRect, "ĐỘ NHẠY CHUỘT", "Tốc độ xoay camera khi di chuyển chuột", -350f,
            0.2f, 3f, playerController != null ? playerController.rotationSpeed : defaultMouseSensitivity,
            SetMouseSensitivity, out mouseSensitivitySlider, out mouseSensitivityValue, false);

        Button backButton = CreateSettingsButton(cardRect, "Back_To_Main_Menu", "QUAY LẠI MENU CHÍNH", CloseSettings);
        RectTransform backRect = backButton.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.5f, 0f);
        backRect.anchorMax = new Vector2(0.5f, 0f);
        backRect.pivot = new Vector2(0.5f, 0f);
        backRect.sizeDelta = new Vector2(360f, 58f);
        backRect.anchoredPosition = new Vector2(0f, 52f);

        SetMasterVolume(masterVolumeSlider.value);
        SetMouseSensitivity(mouseSensitivitySlider.value);
    }

    private void CreateSettingsSection(Transform parent, string heading, string hint, float topOffset,
        float minimum, float maximum, float value, UnityEngine.Events.UnityAction<float> onChanged,
        out Slider slider, out TextMeshProUGUI valueText, bool isPercentage)
    {
        TextMeshProUGUI headingText = CreateText("Label_" + heading, parent, heading, 23f, FontStyles.Bold, Color.white);
        headingText.rectTransform.anchorMin = new Vector2(0f, 1f);
        headingText.rectTransform.anchorMax = new Vector2(0f, 1f);
        headingText.rectTransform.pivot = new Vector2(0f, 1f);
        headingText.rectTransform.sizeDelta = new Vector2(470f, 34f);
        headingText.rectTransform.anchoredPosition = new Vector2(76f, topOffset);
        headingText.characterSpacing = 1.1f;

        valueText = CreateText("Value_" + heading, parent, string.Empty, 23f, FontStyles.Bold, new Color(0.96f, 0.12f, 0.06f));
        valueText.rectTransform.anchorMin = new Vector2(1f, 1f);
        valueText.rectTransform.anchorMax = new Vector2(1f, 1f);
        valueText.rectTransform.pivot = new Vector2(1f, 1f);
        valueText.rectTransform.sizeDelta = new Vector2(110f, 34f);
        valueText.rectTransform.anchoredPosition = new Vector2(-76f, topOffset);
        valueText.alignment = TextAlignmentOptions.MidlineRight;

        TextMeshProUGUI hintText = CreateText("Hint_" + heading, parent, hint, 16f, FontStyles.Italic, new Color(0.75f, 0.67f, 0.63f, 0.85f));
        hintText.rectTransform.anchorMin = new Vector2(0f, 1f);
        hintText.rectTransform.anchorMax = new Vector2(0f, 1f);
        hintText.rectTransform.pivot = new Vector2(0f, 1f);
        hintText.rectTransform.sizeDelta = new Vector2(620f, 26f);
        hintText.rectTransform.anchoredPosition = new Vector2(76f, topOffset - 32f);

        slider = CreateSlider(parent, "Slider_" + heading, minimum, maximum, value);
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 1f);
        sliderRect.anchorMax = new Vector2(0f, 1f);
        sliderRect.pivot = new Vector2(0f, 1f);
        sliderRect.sizeDelta = new Vector2(628f, 34f);
        sliderRect.anchoredPosition = new Vector2(76f, topOffset - 76f);
        slider.onValueChanged.AddListener(onChanged);

        valueText.text = isPercentage
            ? $"{Mathf.RoundToInt(value * 100f)}%"
            : $"{value:0.0}x";
    }

    private static Slider CreateSlider(Transform parent, string objectName, float minimum, float maximum, float value)
    {
        GameObject root = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
        root.transform.SetParent(parent, false);
        Image rootImage = root.GetComponent<Image>();
        rootImage.color = new Color(1f, 1f, 1f, 0.001f);

        Image background = CreateImage("Background", root.transform, new Color(0.18f, 0.035f, 0.03f, 0.96f));
        Stretch(background.rectTransform, new Vector2(0f, 10f), new Vector2(0f, -10f));

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(root.transform, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        Stretch(fillAreaRect, new Vector2(12f, 8f), new Vector2(-12f, -8f));

        Image fill = CreateImage("Fill", fillArea.transform, new Color(0.85f, 0.045f, 0.025f, 1f));
        Stretch(fill.rectTransform, Vector2.zero, Vector2.zero);

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(root.transform, false);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        Stretch(handleAreaRect, new Vector2(12f, 0f), new Vector2(-12f, 0f));

        Image handle = CreateImage("Handle", handleArea.transform, new Color(1f, 0.72f, 0.65f, 1f));
        handle.rectTransform.sizeDelta = new Vector2(20f, 30f);

        Slider slider = root.GetComponent<Slider>();
        slider.minValue = minimum;
        slider.maxValue = maximum;
        slider.value = Mathf.Clamp(value, minimum, maximum);
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private static Button CreateSettingsButton(Transform parent, string objectName, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject root = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);
        Image image = root.GetComponent<Image>();
        image.color = new Color(0.42f, 0.025f, 0.018f, 0.96f);

        Button button = root.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.6f, 0.52f, 1f);
        colors.pressedColor = new Color(0.55f, 0.02f, 0.01f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.onClick.AddListener(onClick);

        TextMeshProUGUI text = CreateText("Label", root.transform, label, 20f, FontStyles.Bold, Color.white);
        Stretch(text.rectTransform, new Vector2(18f, 0f), new Vector2(-18f, 0f));
        text.alignment = TextAlignmentOptions.Center;
        text.characterSpacing = 1.2f;
        return button;
    }

    private static Image CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static TextMeshProUGUI CreateText(string objectName, Transform parent, string content, float fontSize, FontStyles style, Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
