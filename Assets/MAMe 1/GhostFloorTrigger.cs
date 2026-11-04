using UnityEngine;
using System.Collections;

public class GhostDistanceTrigger : MonoBehaviour
{
    [Header("GHOST")]
    [Tooltip("Kéo thả Object con ma vào đây")]
    public GameObject ghostObject;

    [Header("SETTINGS")]
    [Tooltip("Khoảng cách (mét) để kích hoạt con ma")]
    public float triggerDistance = 1f;

    [Tooltip("Thời gian con ma hiển thị (giây)")]
    public float appearDuration = 3f;

    [Tooltip("Kéo thả Object nhân vật (Player) vào đây")]
    public GameObject playerObject;

    [Tooltip("Bật: Chỉ dọa 1 lần. Tắt: Dọa mỗi khi tiến vào vùng 1m.")]
    public bool triggerOnlyOnce = true;

    private bool hasTriggered = false;
    private bool wasInRange = false; // Dùng để nhận biết khoảnh khắc người chơi vừa bước vào vùng 1m
    private Coroutine ghostCoroutine;

    private void Start()
    {
        // Ẩn con ma khi mới bắt đầu game
        if (ghostObject != null)
        {
            ghostObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Chưa gán Ghost Object cho script này!", this);
        }
    }

    private void Update()
    {
        // Bỏ qua nếu chưa gán Player hoặc Ma
        if (playerObject == null || ghostObject == null) return;

        // Tính toán khoảng cách giữa vị trí vật gắn script (tấm ván) và người chơi
        float distance = Vector3.Distance(transform.position, playerObject.transform.position);

        // Kiểm tra xem người chơi có đang ở trong phạm vi 1m không
        bool isInRange = distance <= triggerDistance;

        // Chỉ kích hoạt khi người chơi VỪA tiến vào vùng 1m (ngăn việc nháy liên tục khi đứng im trong vùng đó)
        if (isInRange && !wasInRange)
        {
            TriggerGhost();
        }

        wasInRange = isInRange;
    }

    private void TriggerGhost()
    {
        if (triggerOnlyOnce && hasTriggered)
            return;

        hasTriggered = true;

        if (ghostCoroutine != null)
        {
            StopCoroutine(ghostCoroutine);
        }

        ghostCoroutine = StartCoroutine(ShowGhost());
    }

    private IEnumerator ShowGhost()
    {
        // Hiện ma
        ghostObject.SetActive(true);
        Debug.Log("Player tiến lại gần 1m - Ma xuất hiện!");

        // Chờ theo thời gian đã set (3 giây)
        yield return new WaitForSeconds(appearDuration);

        // Ẩn ma đi
        ghostObject.SetActive(false);
        ghostCoroutine = null;
        Debug.Log("Ma đã biến mất!");
    }
}