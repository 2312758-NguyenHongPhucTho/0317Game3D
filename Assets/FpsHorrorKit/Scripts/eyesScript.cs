using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EyeBlinkFade : MonoBehaviour
{
    [Header("Cấu hình Màn hình")]
    [SerializeField] private Image fadeImage;       // Kéo object FadeScreen vào đây
    [SerializeField] private float delayBeforeOpen = 1f; // Chờ bao lâu rồi mới mở mắt (giây)
    [SerializeField] private float fadeDuration = 2.5f;   // Tốc độ mở mắt (giây)

    private void Start()
    {
        if (fadeImage != null)
        {
            // Bắt đầu với màn hình đen hoàn toàn
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;

            // Bắt đầu hiệu ứng mở mắt
            StartCoroutine(OpenEyesRoutine());
        }
    }

    private IEnumerator OpenEyesRoutine()
    {
        // Chờ một chút trước khi mở mắt
        yield return new WaitForSeconds(delayBeforeOpen);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);

            // Giảm độ mờ (Alpha) của bức ảnh đen
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;

            yield return null;
        }

        // Tắt luôn ảnh đen sau khi mở mắt xong để tiết kiệm tài nguyên
        fadeImage.gameObject.SetActive(false);
    }
}