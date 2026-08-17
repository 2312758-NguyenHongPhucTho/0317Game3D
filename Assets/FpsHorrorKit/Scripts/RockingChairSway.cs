using UnityEngine;
using System.Collections;

public class RockingChairLiftAndDrop : MonoBehaviour
{
    [Header("1. Lắc nhẹ ban đầu")]
    public float rockAngle = 5f;          // Góc lắc nhẹ
    public float rockSpeed = 3f;          // Tốc độ lắc
    public float rockingDuration = 4f;    // Thời gian lắc trước khi bị nhấc

    [Header("2. Nhấc lên & Thả rơi (Giống có người cầm)")]
    public float liftHeight = 0.18f;      // Độ cao nhấc lên rất thấp (vừa đủ nhấc chân ghế khỏi sàn)
    public float liftUpTime = 0.4f;       // Thời gian từ từ nhấc lên (chậm hơn một chút)
    public float dropDownTime = 0.12f;    // Thời gian thả rơi bịch xuống (rất nhanh và nặng)

    [Header("3. Độ lung lay / Chao đảo khi chạm đất")]
    public float landTiltAngle = 10f;     // Góc nghiêng khi chạm đất
    public float wobbleFrequency = 20f;   // Tốc độ chao đảo nẩy chân ghế
    public float wobbleDecay = 5f;        // Tốc độ dập tắt chao đảo
    public float wobbleDuration = 1.2f;   // Thời gian lung lay đến khi đứng yên

    [Header("Bù chìm sàn")]
    public float heightCompensation = 0.12f;

    [Header("Thời gian chờ")]
    public float startDelay = 1f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;

        StartCoroutine(SequenceRoutine());
    }

    IEnumerator SequenceRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        // ================================================
        // GIAI ĐOẠN 1: LẮC NHẸ NHÀNG
        // ================================================
        float timer = 0f;
        float phase = 0f;

        while (timer < rockingDuration)
        {
            timer += Time.deltaTime;
            phase += Time.deltaTime * rockSpeed;

            float rotX = Mathf.Sin(phase) * rockAngle;
            transform.localRotation = startRotation * Quaternion.Euler(rotX, 0f, 0f);

            // Bù độ cao chống xuyên đất
            float yOffset = Mathf.Abs(Mathf.Sin(rotX * Mathf.Deg2Rad)) * heightCompensation;
            transform.localPosition = startPosition + Vector3.up * yOffset;

            yield return null;
        }

        // ================================================
        // GIAI ĐOẠN 2A: TỪ TỪ NHẤC LÊN (Giống có người cầm nhấc nhẹ)
        // ================================================
        float liftTimer = 0f;
        while (liftTimer < liftUpTime)
        {
            liftTimer += Time.deltaTime;
            float t = liftTimer / liftUpTime;

            // Dùng SmoothStep để tay nhấc lên mượt mà, không bị giật cục
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float currentHeight = smoothT * liftHeight;

            transform.localPosition = startPosition + Vector3.up * currentHeight;

            // Chúc đầu ghế nhẹ khi được nhấc lên
            float liftTilt = smoothT * (landTiltAngle * 0.5f);
            transform.localRotation = startRotation * Quaternion.Euler(liftTilt, 0f, 0f);

            yield return null;
        }

        // ================================================
        // GIAI ĐOẠN 2B: THẢ RƠI TỰ DO (Rơi bịch xuống sàn ngay lập tức)
        // ================================================
        float dropTimer = 0f;
        while (dropTimer < dropDownTime)
        {
            dropTimer += Time.deltaTime;
            float t = dropTimer / dropDownTime;

            // Bình phương thời gian tạo gia tốc trọng lực rơi rớt cực nhanh
            float dropProgress = 1f - (t * t);
            float currentHeight = dropProgress * liftHeight;

            transform.localPosition = startPosition + Vector3.up * currentHeight;

            yield return null;
        }

        // ================================================
        // GIAI ĐOẠN 3: CHẠM ĐẤT VÀ LUNG LAY TẮT DẦN (WOBBLE)
        // ================================================
        float wobbleTimer = 0f;

        while (wobbleTimer < wobbleDuration)
        {
            wobbleTimer += Time.deltaTime;

            float damping = Mathf.Exp(-wobbleDecay * wobbleTimer);
            float currentWobble = Mathf.Sin(wobbleTimer * wobbleFrequency) * landTiltAngle * damping;

            transform.localRotation = startRotation * Quaternion.Euler(currentWobble, 0f, 0f);

            float yOffset = Mathf.Abs(Mathf.Sin(currentWobble * Mathf.Deg2Rad)) * heightCompensation;
            transform.localPosition = startPosition + Vector3.up * yOffset;

            yield return null;
        }

        // Hoàn tất trả về gốc
        transform.localPosition = startPosition;
        transform.localRotation = startRotation;
    }
}