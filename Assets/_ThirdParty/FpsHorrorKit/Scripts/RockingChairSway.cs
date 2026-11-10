using UnityEngine;
using System.Collections;

public class RockingChairLiftAndDrop : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Khoang cach kich hoat")]
    public float activationDistance = 1f;

    [Header("Manual trigger")]
    [Tooltip("Bat de ghe chi chay khi script khac goi TriggerRocking().")]
    public bool triggerOnly;

    [Header("Thoi gian lac")]
    public float rockingDuration = 30f;

    [Header("Chuyen dong lac ghe")]
    public float rockAngle = 6f;
    public float rockSpeed = 3f;

    [Header("Do muot khi lac")]
    public float smoothSpeed = 3f;

    [Header("Bu do cao tranh chim san")]
    public float heightCompensation = 0.12f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool hasTriggered = false;
    private bool isRocking = false;

    private float rockTimer = 0f;
    private float currentRockAmount = 0f;

    void Start()
    {
        // Luu vi tri va goc xoay ban dau
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;

        // Tu dong tim camera cua nguoi choi
        if (player == null && Camera.main != null)
        {
            player = Camera.main.transform;
        }
    }

    void Update()
    {
        // Neu da kich hoat roi thi khong kiem tra lai
        if (triggerOnly || hasTriggered || player == null)
            return;

        // Tinh khoang cach giua player va ghe
        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Khi nguoi choi den gan 1 met
        if (distance <= activationDistance)
        {
            hasTriggered = true;

            StartCoroutine(RockingSequence());
        }
    }

    public void TriggerRocking()
    {
        if (isRocking || hasTriggered)
            return;

        hasTriggered = true;
        StartCoroutine(RockingSequence());
    }

    IEnumerator RockingSequence()
    {
        isRocking = true;

        float elapsed = 0f;

        // Lac lien tuc trong 30 giay
        while (elapsed < rockingDuration)
        {
            elapsed += Time.deltaTime;

            // Tang do lac tu tu
            currentRockAmount = Mathf.MoveTowards(
                currentRockAmount,
                1f,
                smoothSpeed * Time.deltaTime
            );

            rockTimer += Time.deltaTime * rockSpeed;

            float rotX =
                Mathf.Sin(rockTimer) *
                rockAngle *
                currentRockAmount;

            // Lac ghe qua lai
            transform.localRotation =
                startRotation *
                Quaternion.Euler(rotX, 0f, 0f);

            // Bu do cao de tranh chim san
            float yOffset =
                Mathf.Abs(
                    Mathf.Sin(rotX * Mathf.Deg2Rad)
                ) * heightCompensation;

            transform.localPosition =
                startPosition + Vector3.up * yOffset;

            yield return null;
        }

        // Het 30 giay thi dung lac tu tu
        while (currentRockAmount > 0.001f)
        {
            currentRockAmount = Mathf.MoveTowards(
                currentRockAmount,
                0f,
                smoothSpeed * Time.deltaTime
            );

            rockTimer += Time.deltaTime * rockSpeed;

            float rotX =
                Mathf.Sin(rockTimer) *
                rockAngle *
                currentRockAmount;

            transform.localRotation =
                startRotation *
                Quaternion.Euler(rotX, 0f, 0f);

            float yOffset =
                Mathf.Abs(
                    Mathf.Sin(rotX * Mathf.Deg2Rad)
                ) * heightCompensation;

            transform.localPosition =
                startPosition + Vector3.up * yOffset;

            yield return null;
        }

        // Tra ghe ve trang thai ban dau
        transform.localPosition = startPosition;
        transform.localRotation = startRotation;

        currentRockAmount = 0f;
        isRocking = false;

        // hasTriggered van = true
        // Nen khong kich hoat lai
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            activationDistance
        );
    }
}
