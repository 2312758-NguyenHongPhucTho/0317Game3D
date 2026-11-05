using System.Collections;
using FpsHorrorKit;
using UnityEngine;
using UnityEngine.UI;

public class GhostAutoCanvasJumpscare : MonoBehaviour
{
    [Header("TARGETS")]
    [Tooltip("Kéo Player thật (object có PlayerHealth0317) vào đây.")]
    public GameObject playerObject;

    [Tooltip("Ảnh con ma, import dưới dạng Sprite (2D and UI).")]
    public Sprite ghostSprite;

    [Header("DAMAGE")]
    [Tooltip("Component máu sẵn có trên Player. Nếu bỏ trống, script sẽ tự tìm từ Player Object.")]
    public PlayerHealth0317 playerHealth;

    [Min(1)] public int damageAmount = 10;

    [Header("SETTINGS")]
    [Min(0.1f)] public float triggerDistance = 1.5f;
    [Min(0f)] public float delayBeforeShow = 3f;
    [Min(0.1f)] public float showDuration = 2f;
    public bool triggerOnlyOnce = true;

    private bool hasTriggered;
    private bool isSequenceRunning;
    private GameObject autoCanvas;
    private Coroutine sequence;

    private void Start()
    {
        FindPlayerHealth();
        CreateFullscreenCanvas();
    }

    private void FindPlayerHealth()
    {
        if (playerHealth == null && playerObject != null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth0317>();
            if (playerHealth == null)
                playerHealth = playerObject.GetComponentInParent<PlayerHealth0317>();
        }
    }

    private void CreateFullscreenCanvas()
    {
        if (ghostSprite == null)
        {
            Debug.LogError("[03:17] Chưa gán Ghost Sprite cho jumpscare!", this);
            return;
        }

        autoCanvas = new GameObject("AutoJumpscareCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas canvasComp = autoCanvas.GetComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComp.sortingOrder = 999;

        GameObject imageObj = new GameObject("GhostImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObj.transform.SetParent(autoCanvas.transform, false);

        Image imgComp = imageObj.GetComponent<Image>();
        imgComp.sprite = ghostSprite;
        imgComp.raycastTarget = false;

        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        autoCanvas.SetActive(false);
    }

    private void Update()
    {
        if (playerObject == null || autoCanvas == null) return;
        if (triggerOnlyOnce && hasTriggered) return;
        if (isSequenceRunning) return;

        float distance = Vector3.Distance(transform.position, playerObject.transform.position);
        if (distance <= triggerDistance)
            sequence = StartCoroutine(JumpscareSequence());
    }

    private IEnumerator JumpscareSequence()
    {
        hasTriggered = true;
        isSequenceRunning = true;

        yield return new WaitForSeconds(delayBeforeShow);

        autoCanvas.SetActive(true);
        DealDamageOnceForThisJumpscare();
        Debug.Log("[03:17] Jumpscare xuất hiện!", this);

        yield return new WaitForSeconds(showDuration);

        autoCanvas.SetActive(false);
        isSequenceRunning = false;
        sequence = null;
    }

    private void DealDamageOnceForThisJumpscare()
    {
        FindPlayerHealth();
        if (playerHealth == null)
        {
            Debug.LogWarning("[03:17] Không tìm thấy PlayerHealth0317: jumpscare không trừ HP.", this);
            return;
        }

        // Dùng hệ thống HP hiện có, bao gồm miễn sát thương ngắn và xử lý HP = 0.
        if (playerHealth.TakeTrapDamage(damageAmount))
            Debug.Log($"[03:17] Jumpscare gây {damageAmount} sát thương.", this);
    }

    private void OnDestroy()
    { if (gameObject != null)
        if (autoCanvas != null) Destroy(autoCanvas);
    }

    private void OnDisable()
    {
        if (sequence != null)
        {
            StopCoroutine(sequence);
            sequence = null;
        }
        isSequenceRunning = false;
        if (autoCanvas != null) autoCanvas.SetActive(false);
    }
}
