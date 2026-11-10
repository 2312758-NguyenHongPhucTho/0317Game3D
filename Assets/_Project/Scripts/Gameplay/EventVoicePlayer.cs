using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class EventVoicePlayer : MonoBehaviour
{
    [Header("1. Danh sach file am thanh")]
    public AudioClip[] voiceClips;

    [Header("2. NHAP TEXT BANG TAY")]
    [TextArea(3, 8)]
    public string manualText;

    [Header("3. Text cho tung file am thanh")]
    [TextArea(2, 5)]
    public string[] subtitleLines;

    [Header("4. Cai dat su kien")]
    public bool playOnStart = false;
    public bool playOnce = true;

    [Header("5. Kich hoat khi den gan")]
    public bool triggerByDistance = false;
    public Transform player;
    public float triggerDistance = 2f;

    [Header("6. Thoi gian")]
    public float startDelay = 0f;
    public float delayBetweenClips = 0.5f;

    [Header("7. Kieu chu")]
    public TMP_FontAsset subtitleFont;
    public FontStyles fontStyle = FontStyles.Normal;

    [Header("8. Mau va kich thuoc")]
    public Color textColor = Color.white;
    public float fontSize = 36f;

    [Header("9. Vi tri chu")]
    public Vector2 textPosition = new Vector2(0f, 120f);

    [Header("10. Kich thuoc khung")]
    public Vector2 textBoxSize = new Vector2(1000f, 150f);

    [Header("11. Toc do chay chu")]
    [Min(1f)]
    public float charactersPerSecond = 25f;

    [Header("12. Thoi gian text neu khong co am thanh")]
    public float textOnlyDuration = 4f;

    [Header("13. Giu chu sau khi ket thuc")]
    public float subtitleHoldTime = 0.5f;

    private AudioSource audioSource;
    private TextMeshProUGUI subtitleText;

    // Canvas va Text dung chung cho cac su kien
    private static TextMeshProUGUI sharedSubtitle;
    private static EventVoicePlayer currentEvent;

    private bool hasPlayed = false;
    private bool isPlaying = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Start()
    {
        if (player == null && Camera.main != null)
        {
            player = Camera.main.transform;
        }

        if (playOnStart)
        {
            PlayEvent();
        }
    }

    void Update()
    {
        if (!triggerByDistance)
            return;

        if (player == null || isPlaying)
            return;

        if (playOnce && hasPlayed)
            return;

        Vector3 offset = player.position - transform.position;

        // Tinh khoang cach ngang
        offset.y = 0f;

        if (offset.magnitude <= triggerDistance)
        {
            PlayEvent();
        }
    }

    // Ham kich hoat su kien
    public void PlayEvent()
    {
        if (isPlaying)
            return;

        if (playOnce && hasPlayed)
            return;

        int count = GetEntryCount();

        if (count == 0)
            return;

        // Dung loi thoai cu neu su kien moi bat dau
        if (currentEvent != null && currentEvent != this)
        {
            currentEvent.StopEvent();
        }

        currentEvent = this;

        // TU DONG TAO PHU DE
        subtitleText = GetOrCreateSubtitle();

        ApplyTextSettings();

        isPlaying = true;
        hasPlayed = true;

        StartCoroutine(PlaySequence(count));
    }

    int GetEntryCount()
    {
        int audioCount =
            voiceClips != null ? voiceClips.Length : 0;

        int textCount =
            subtitleLines != null ? subtitleLines.Length : 0;

        int count = Mathf.Max(audioCount, textCount);

        if (count == 0 &&
            !string.IsNullOrWhiteSpace(manualText))
        {
            count = 1;
        }

        return count;
    }

    string GetSubtitle(int index)
    {
        if (subtitleLines != null &&
            index < subtitleLines.Length &&
            !string.IsNullOrWhiteSpace(subtitleLines[index]))
        {
            return subtitleLines[index];
        }

        if (index == 0)
        {
            return manualText;
        }

        return "";
    }

    // TU DONG TAO CANVAS VA TEXTMESHPRO
    TextMeshProUGUI GetOrCreateSubtitle()
    {
        if (sharedSubtitle != null)
        {
            return sharedSubtitle;
        }

        // Tao Canvas
        GameObject canvasObject =
            new GameObject("03_17_SubtitleCanvas");

        Canvas canvas = canvasObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        // Tu dong can theo do phan giai
        CanvasScaler scaler =
            canvasObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1920f, 1080f);

        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        scaler.matchWidthOrHeight = 0.5f;

        // Tao doi tuong chu
        GameObject textObject =
            new GameObject(
                "SubtitleText",
                typeof(RectTransform)
            );

        textObject.transform.SetParent(
            canvasObject.transform,
            false
        );

        sharedSubtitle =
            textObject.AddComponent<TextMeshProUGUI>();

        sharedSubtitle.font = TMP_Settings.defaultFontAsset;

        sharedSubtitle.raycastTarget = false;

        sharedSubtitle.alignment =
            TextAlignmentOptions.Center;

        sharedSubtitle.text = "";

        return sharedSubtitle;
    }

    void ApplyTextSettings()
    {
        if (subtitleText == null)
            return;

        if (subtitleFont != null)
        {
            subtitleText.font = subtitleFont;
        }

        subtitleText.fontStyle = fontStyle;
        subtitleText.color = textColor;
        subtitleText.fontSize = fontSize;

        subtitleText.alignment =
            TextAlignmentOptions.Center;

        subtitleText.enableAutoSizing = false;

        RectTransform rect = subtitleText.rectTransform;

        // Neo o giua phia duoi man hinh
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);

        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.anchoredPosition = textPosition;
        rect.sizeDelta = textBoxSize;
    }

    IEnumerator PlaySequence(int count)
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        for (int i = 0; i < count; i++)
        {
            AudioClip clip = null;

            if (voiceClips != null && i < voiceClips.Length)
            {
                clip = voiceClips[i];
            }

            string text = GetSubtitle(i);

            if (clip == null && string.IsNullOrWhiteSpace(text))
                continue;

            yield return StartCoroutine(
                PlayClipWithSubtitle(clip, text)
            );

            if (i < count - 1 && delayBetweenClips > 0f)
            {
                yield return new WaitForSeconds(
                    delayBetweenClips
                );
            }
        }

        isPlaying = false;

        if (currentEvent == this)
        {
            currentEvent = null;
        }
    }

    IEnumerator PlayClipWithSubtitle(AudioClip clip, string text)
    {
        bool showText =
            subtitleText != null &&
            !string.IsNullOrWhiteSpace(text);

        int totalCharacters = 0;

        if (showText)
        {
            subtitleText.text = text;
            subtitleText.ForceMeshUpdate();

            totalCharacters =
                subtitleText.textInfo.characterCount;

            subtitleText.maxVisibleCharacters = 0;
        }
        else
        {
            HideSubtitle();
        }

        bool hasAudio = clip != null;

        if (hasAudio)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }

        float elapsed = 0f;

        float textDuration = Mathf.Max(
            textOnlyDuration,
            totalCharacters / Mathf.Max(1f, charactersPerSecond)
        );

        // Hien chu theo thoi gian phat am thanh
        while (hasAudio
            ? audioSource.isPlaying
            : elapsed < textDuration)
        {
            elapsed += Time.deltaTime;

            if (showText)
            {
                int visibleCount = Mathf.FloorToInt(
                    elapsed * charactersPerSecond
                );

                subtitleText.maxVisibleCharacters =
                    Mathf.Min(visibleCount, totalCharacters);
            }

            yield return null;
        }

        if (showText)
        {
            // Hien het chu con lai
            subtitleText.maxVisibleCharacters = int.MaxValue;

            if (subtitleHoldTime > 0f)
            {
                yield return new WaitForSeconds(
                    subtitleHoldTime
                );
            }
        }

        HideSubtitle();
    }

    void HideSubtitle()
    {
        if (subtitleText != null && currentEvent == this)
        {
            subtitleText.text = "";
            subtitleText.maxVisibleCharacters = int.MaxValue;
        }
    }

    public void StopEvent()
    {
        StopAllCoroutines();

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        HideSubtitle();

        isPlaying = false;

        if (currentEvent == this)
        {
            currentEvent = null;
        }
    }

    void OnDisable()
    {
        StopEvent();
    }
}