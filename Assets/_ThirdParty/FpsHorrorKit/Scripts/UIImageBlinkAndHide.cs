
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class UIImageBlinkAndHide : MonoBehaviour
{
    [Header("Fit vao Canvas")]
    public bool fitFullCanvas = true;
    public bool preserveAspect = false;

    [Header("Thoi gian cho")]
    public float startDelay = 2f;

    [Header("Hieu ung nhay")]
    public bool playOnStart = true;
    public float blinkInterval = 0.15f;
    public float blinkDuration = 2f;

    [Header("Bien mat")]
    public bool hideAfterBlink = true;
    public bool disableObjectAfterHide = true;

    private Image img;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        img = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // An anh khi bat dau game
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    void Start()
    {
        if (fitFullCanvas)
            FitToCanvas();

        img.preserveAspect = preserveAspect;

        if (playOnStart)
            StartCoroutine(StartEffect());
    }

    IEnumerator StartEffect()
    {
        // Cho 2 giay roi moi hien anh
        yield return new WaitForSeconds(startDelay);

        yield return StartCoroutine(BlinkAndHide());
    }

    void FitToCanvas()
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    public IEnumerator BlinkAndHide()
    {
        float timer = 0f;
        bool visible = true;

        float interval = Mathf.Max(0.01f, blinkInterval);

        // Hien anh khi bat dau hieu ung
        canvasGroup.alpha = 1f;

        while (timer < blinkDuration)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            visible = !visible;

            yield return new WaitForSeconds(interval);
            timer += interval;
        }

        if (hideAfterBlink)
        {
            canvasGroup.alpha = 0f;

            if (disableObjectAfterHide)
                gameObject.SetActive(false);
        }
        else
        {
            canvasGroup.alpha = 1f;
        }
    }
}
