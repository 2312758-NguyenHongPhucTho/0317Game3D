
using UnityEngine;
using System.Collections;

public class HorrorLightFlicker : MonoBehaviour
{
    [Header("Den")]
    public Light targetLight;

    [Header("Nguoi choi")]
    public Transform player;
    public float triggerDistance = 5f;

    [Header("Hieu ung nhap nhay")]
    public float minFlickerInterval = 0.05f;
    public float maxFlickerInterval = 0.2f;
    public float flickerDuration = 3f;

    [Header("Trang thai")]
    public bool turnOffAfterFlicker = true;
    public bool triggerOnlyOnce = true;

    private bool isFlickering = false;
    private bool hasTriggered = false;

    void Start()
    {
        if (targetLight == null)
            targetLight = GetComponentInChildren<Light>();

        if (player == null && Camera.main != null)
            player = Camera.main.transform;
    }

    void Update()
    {
        if (player == null || targetLight == null)
            return;

        if (isFlickering)
            return;

        if (triggerOnlyOnce && hasTriggered)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= triggerDistance)
        {
            StartCoroutine(FlickerRoutine());
        }
    }

    IEnumerator FlickerRoutine()
    {
        isFlickering = true;
        hasTriggered = true;

        float elapsed = 0f;

        float minInterval = Mathf.Max(
            0.01f, minFlickerInterval
        );

        float maxInterval = Mathf.Max(
            minInterval, maxFlickerInterval
        );

        while (elapsed < flickerDuration)
        {
            targetLight.enabled = !targetLight.enabled;

            float interval = Random.Range(
                minInterval,
                maxInterval
            );

            yield return new WaitForSeconds(interval);

            elapsed += interval;
        }

        targetLight.enabled = !turnOffAfterFlicker;

        isFlickering = false;
    }
}
