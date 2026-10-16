using System.Collections;
using UnityEngine;

public class GhostSilhouetteHorrorEvent : HorrorEventTrigger
{
    [SerializeField] private GhostActor ghost;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float visibleDuration = 1.5f;

    private void OnValidate()
    {
        ValidateDuration();
    }

    private void ValidateDuration()
    {
        visibleDuration = float.IsNaN(visibleDuration) || float.IsInfinity(visibleDuration)
            ? 0f : Mathf.Max(0f, visibleDuration);
    }

    protected override IEnumerator PlaySequence()
    {
        if (ghost == null || spawnPoint == null)
            yield break;

        ValidateDuration();
        GhostActor actor = ghost;
        actor.ShowAt(spawnPoint);
        try
        {
            if (visibleDuration > 0f)
                yield return new WaitForSeconds(visibleDuration);
        }
        finally
        {
            if (actor != null)
                actor.Hide();
        }
    }
}
