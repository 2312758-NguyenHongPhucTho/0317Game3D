using UnityEngine;
using UnityEngine.UI; // Cần thiết để code tự tạo UI Image
using System.Collections;

public class GhostAutoCanvasJumpscare : MonoBehaviour
{
    [Header("TARGETS")]
    [Tooltip("Kéo thả Object Player vào đây")]
    public GameObject playerObject;

    [Tooltip("Kéo file ảnh con ma vào đây (Nhớ đổi ảnh thành đuôi Sprite 2D)")]
    public Sprite ghostSprite;

    [Header("SETTINGS")]
    [Tooltip("Khoảng cách kích hoạt (mét)")]
    public float triggerDistance = 1.5f;

    [Tooltip("Thời gian chờ trước khi ma xuất hiện (giây)")]
    public float delayBeforeShow = 3f;

    [Tooltip("Thời gian hình ma hiển thị trên màn hình (giây)")]
    public float showDuration = 2f;

    [Tooltip("Bật: Chỉ dọa 1 lần duy nhất trong toàn bộ màn chơi")]
    public bool triggerOnlyOnce = true;

    private bool hasTriggered = false;
    private bool isSequenceRunning = false;

    // Biến lưu trữ Canvas do code tự tạo ra
    private GameObject autoCanvas;

    private void Start()
    {
        CreateFullscreenCanvas();
    }

    // Hàm này giúp tự động tạo Canvas và Image full màn hình bằng Code
    private void CreateFullscreenCanvas()
    {
        if (ghostSprite == null)
        {
            Debug.LogError("Bạn chưa kéo ảnh con ma vào ô Ghost Sprite trên Inspector!", this);
            return;
        }

        // 1. Tạo Canvas
        autoCanvas = new GameObject("AutoJumpscareCanvas");
        Canvas canvasComp = autoCanvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComp.sortingOrder = 999; // Đặt số thật cao để ảnh ma đè lên mọi UI khác

        autoCanvas.AddComponent<CanvasScaler>();
        autoCanvas.AddComponent<GraphicRaycaster>();

        // 2. Tạo Image con chứa ảnh ma
        GameObject imageObj = new GameObject("GhostImage");
        imageObj.transform.SetParent(autoCanvas.transform, false);

        Image imgComp = imageObj.AddComponent<Image>();
        imgComp.sprite = ghostSprite;

        // 3. Căn chỉnh Image tự động kéo giãn full màn hình
        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0); // Góc dưới trái
        rect.anchorMax = new Vector2(1, 1); // Góc trên phải
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // 4. Ẩn Canvas đi, chờ khi nào Player tới gần mới bật
        autoCanvas.SetActive(false);
    }

    private void Update()
    {
        if (playerObject == null || autoCanvas == null) return;
        if (triggerOnlyOnce && hasTriggered) return;
        if (isSequenceRunning) return;

        // Tính khoảng cách
        float distance = Vector3.Distance(transform.position, playerObject.transform.position);

        if (distance <= triggerDistance)
        {
            StartCoroutine(JumpscareSequence());
        }
    }

    private IEnumerator JumpscareSequence()
    {
        hasTriggered = true;
        isSequenceRunning = true;

        // Chờ 3 giây
        yield return new WaitForSeconds(delayBeforeShow);

        // Bật màn hình
        autoCanvas.SetActive(true);
        Debug.Log("Jumpscare!");

        // Hiện trong 2 giây
        yield return new WaitForSeconds(showDuration);

        // Tắt màn hình
        autoCanvas.SetActive(false);

        isSequenceRunning = false;
    }
}