using System.Collections;
using UnityEngine;

public class LightFlickerHorrorEvent : HorrorEventTrigger
{
    [SerializeField] private Light targetLight;
    [SerializeField] private float duration = 2f;
    [SerializeField] private float minInterval = 0.05f;
    [SerializeField] private float maxInterval = 0.2f;

    private void OnValidate()
    {
        ValidateTiming();
    }

    private void ValidateTiming()
    {
        duration = SafeValue(duration, 0f);
        minInterval = SafeValue(minInterval, 0.01f);
        maxInterval = Mathf.Max(minInterval, SafeValue(maxInterval, 0.01f));
    }

    private static float SafeValue(float value, float minimum)
    {
        return float.IsNaN(value) || float.IsInfinity(value)
            ? minimum : Mathf.Max(minimum, value);
    }

    protected override IEnumerator PlaySequence()
    {
        if (targetLight == null)
            yield break;

        ValidateTiming();
        Light light = targetLight;
        bool initialEnabled = light.enabled;
        float elapsed = 0f;
        try
        {
            while (elapsed < duration && light != null)
            {
                light.enabled = !light.enabled;
                float interval = Mathf.Min(Random.Range(minInterval, maxInterval), duration - elapsed);
                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }
        }
        finally
        {
            if (light != null)
                light.enabled = initialEnabled;
        }
    }
}
