using UnityEngine;
using System.Collections;

public class ShrineLightFlicker : MonoBehaviour
{
    [Header("Danh sach den trong phong tho")]
    public Light[] lights;

    [Header("Do sang binh thuong")]
    [Range(0f, 2f)]
    public float normalBrightness = 1f;

    [Range(0f, 1f)]
    public float idleFlickerAmount = 0.08f;

    public float idleFlickerSpeed = 4f;

    [Header("Thoi gian giua cac dot nhay")]
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("So lan nhay moi dot")]
    public int minFlickers = 2;
    public int maxFlickers = 5;

    [Header("Do toi khi den nhay")]
    [Range(0f, 1f)]
    public float minDarkBrightness = 0.05f;

    [Range(0f, 1f)]
    public float maxDarkBrightness = 0.3f;

    [Header("Toc do nhay")]
    public float minFlickerTime = 0.04f;
    public float maxFlickerTime = 0.15f;

    private float[] originalIntensities;
    private Coroutine flickerRoutine;

    private void Awake()
    {
        // Neu khong gan den, tu tim trong GameObject
        // va cac GameObject con
        if (lights == null || lights.Length == 0)
        {
            lights = GetComponentsInChildren<Light>(true);
        }

        originalIntensities = new float[lights.Length];

        // Luu do sang ban dau cua tung den
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                originalIntensities[i] = lights[i].intensity;
            }
        }
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            flickerRoutine = StartCoroutine(FlickerRoutine());
        }
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // GIAI DOAN 1:
            // Den sang binh thuong, dao dong nhe

            float waitTime = Random.Range(
                Mathf.Min(minWaitTime, maxWaitTime),
                Mathf.Max(minWaitTime, maxWaitTime)
            );

            float elapsed = 0f;

            while (elapsed < waitTime)
            {
                elapsed += Time.deltaTime;

                float noise = Mathf.PerlinNoise(
                    Time.time * idleFlickerSpeed,
                    0.5f
                );

                float brightness =
                    normalBrightness *
                    Mathf.Lerp(
                        1f - idleFlickerAmount,
                        1f,
                        noise
                    );

                SetBrightness(brightness);

                yield return null;
            }

            // GIAI DOAN 2:
            // Den bat dau nhay lien tuc

            int flickerCount = Random.Range(
                Mathf.Max(1, minFlickers),
                Mathf.Max(minFlickers, maxFlickers) + 1
            );

            for (int i = 0; i < flickerCount; i++)
            {
                // Den toi sam
                float darkBrightness = Random.Range(
                    minDarkBrightness,
                    maxDarkBrightness
                );

                SetBrightness(darkBrightness);

                float darkTime = Random.Range(
                    Mathf.Min(minFlickerTime, maxFlickerTime),
                    Mathf.Max(minFlickerTime, maxFlickerTime)
                );

                yield return new WaitForSeconds(
                    Mathf.Max(0.01f, darkTime)
                );

                // Den bat sang lai
                SetBrightness(normalBrightness);

                float brightTime = Random.Range(
                    Mathf.Min(minFlickerTime, maxFlickerTime),
                    Mathf.Max(minFlickerTime, maxFlickerTime)
                );

                yield return new WaitForSeconds(
                    Mathf.Max(0.01f, brightTime)
                );
            }

            // Khoi phuc do sang ban dau
            SetBrightness(normalBrightness);
        }
    }

    private void SetBrightness(float multiplier)
    {
        if (lights == null || originalIntensities == null)
            return;

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] == null)
                continue;

            lights[i].intensity =
                originalIntensities[i] *
                Mathf.Max(0f, multiplier);
        }
    }

    private void OnDisable()
    {
        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }

        RestoreLights();
    }

    private void OnDestroy()
    {
        RestoreLights();
    }

    private void RestoreLights()
    {
        if (lights == null || originalIntensities == null)
            return;

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                lights[i].intensity = originalIntensities[i];
            }
        }
    }
}