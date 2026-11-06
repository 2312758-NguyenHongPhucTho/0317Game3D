using UnityEngine;

/// <summary>
/// Khi Player den gan lan can cau thang, bat con ma Opening.
/// MotherGhostMovement tren Opening (hoac con cua Opening) tiep tuc xu ly
/// animation, am thanh, di chuyen va sat thuong nhu binh thuong.
/// </summary>
public sealed class StairsRailingGhostTrigger : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject openingGhost;

    [Header("TRIGGER")]
    [SerializeField, Min(0.1f)] private float triggerDistance = 3f;
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (openingGhost == null)
        {
            Debug.LogError("[03:17] Chua gan Opening Ghost cho StairsRailingGhostTrigger!", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (triggerOnlyOnce && hasTriggered) return;
        if (player == null || openingGhost == null) return;
        if (Time.timeScale <= 0f) return;

        // Da hien thi thi khong kich hoat lai moi frame.
        if (openingGhost.activeInHierarchy) return;

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        if (offset.sqrMagnitude > triggerDistance * triggerDistance) return;

        ActivateGhost();
    }

    private void ActivateGhost()
    {
        hasTriggered = true;

        // Cau hinh tham chieu truoc khi SetActive(true) kich hoat Awake/OnEnable.
        MotherGhostMovement movement =
            openingGhost.GetComponentInChildren<MotherGhostMovement>(true);

        if (movement != null)
        {
            if (movement.player == null)
                movement.player = player;

            if (movement.ghostObject == null)
                movement.ghostObject = openingGhost;

            movement.enabled = true;
        }
        else
        {
            Debug.LogWarning(
                "[03:17] Opening Ghost chua co MotherGhostMovement tren chinh no hoac object con.", this);
        }

        openingGhost.SetActive(true);

        if (!openingGhost.activeInHierarchy)
            Debug.LogWarning("[03:17] Opening Ghost van an do parent dang inactive.", this);
        else
            Debug.Log("[03:17] Player den gan cau thang: ma xuat hien, MotherGhostMovement bat dau hoat dong.", this);
    }
}
