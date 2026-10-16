using System.Collections;
using UnityEngine;

public class GhostApparitionHorrorEvent : HorrorEventTrigger
{
    [SerializeField] private GhostActor ghost;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Light targetLight;
    [SerializeField] private AudioSource apparitionAudio;
    [SerializeField] private float preDelay = 1f;
    [SerializeField] private float visibleDuration = 2f;
    [SerializeField] private float flickerDuration = 1f;
    [SerializeField] private float minFlickerInterval = 0.05f;
    [SerializeField] private float maxFlickerInterval = 0.15f;

    private void OnValidate()
    {
        ValidateTiming();
    }

    private static float SafeDuration(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value) ? 0f : Mathf.Max(0f, value);
    }

    private void ValidateTiming()
    {
        preDelay = SafeDuration(preDelay);
        visibleDuration = SafeDuration(visibleDuration);
        flickerDuration = SafeDuration(flickerDuration);
        // A positive minimum prevents a zero-interval loop from stalling a frame.
        minFlickerInterval = Mathf.Max(0.01f, SafeDuration(minFlickerInterval));
        maxFlickerInterval = Mathf.Max(minFlickerInterval, SafeDuration(maxFlickerInterval));
    }

    protected override IEnumerator PlaySequence()
    {
        ValidateTiming();
        if (preDelay > 0f)
            yield return new WaitForSeconds(preDelay);

        Light light = targetLight;
        bool initialLightEnabled = light != null && light.enabled;
        GhostActor shownGhost = null;
        try
        {
            float elapsed = 0f;
            while (light != null && elapsed < flickerDuration)
            {
                light.enabled = !light.enabled;
                float interval = Mathf.Min(Random.Range(minFlickerInterval, maxFlickerInterval), flickerDuration - elapsed);
                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }

            if (ghost != null && spawnPoint != null)
            {
                shownGhost = ghost;
                shownGhost.ShowAt(spawnPoint);
            }

            if (apparitionAudio != null && apparitionAudio.clip != null)
                apparitionAudio.Play();

            if (visibleDuration > 0f)
                yield return new WaitForSeconds(visibleDuration);
        }
        finally
        {
            if (shownGhost != null)
                shownGhost.Hide();
            if (light != null)
                light.enabled = initialLightEnabled;
        }
    }
}
