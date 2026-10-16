using System.Collections;
using UnityEngine;

public class BasementHorrorEvent : HorrorEventTrigger
{
    [SerializeField] private GhostActor ghost;
    [SerializeField] private Transform firstSpawnPoint;
    [SerializeField] private Transform secondSpawnPoint;
    [SerializeField] private Light targetLight;
    [SerializeField] private AudioSource basementAudio;
    [SerializeField] private float preDelay = 1f;
    [SerializeField] private float firstVisibleDuration = 1.25f;
    [SerializeField] private float blackoutDuration = 0.5f;
    [SerializeField] private float secondVisibleDuration = 1.5f;

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
        firstVisibleDuration = SafeDuration(firstVisibleDuration);
        blackoutDuration = SafeDuration(blackoutDuration);
        secondVisibleDuration = SafeDuration(secondVisibleDuration);
    }

    protected override IEnumerator PlaySequence()
    {
        ValidateTiming();
        Light light = targetLight;
        bool initialLightEnabled = light != null && light.enabled;
        GhostActor actor = ghost;
        try
        {
            if (preDelay > 0f)
                yield return new WaitForSeconds(preDelay);

            if (actor != null && firstSpawnPoint != null)
            {
                actor.ShowAt(firstSpawnPoint);
            }
            if (firstVisibleDuration > 0f)
                yield return new WaitForSeconds(firstVisibleDuration);
            if (actor != null)
                actor.Hide();

            if (light != null)
                light.enabled = false;
            if (blackoutDuration > 0f)
                yield return new WaitForSeconds(blackoutDuration);

            if (actor != null && secondSpawnPoint != null)
            {
                actor.ShowAt(secondSpawnPoint);
            }
            if (light != null)
                light.enabled = initialLightEnabled;
            if (basementAudio != null && basementAudio.clip != null)
                basementAudio.Play();
            if (secondVisibleDuration > 0f)
                yield return new WaitForSeconds(secondVisibleDuration);
        }
        finally
        {
            if (actor != null)
                actor.Hide();
            if (light != null)
                light.enabled = initialLightEnabled;
        }
    }
}
