using System.Collections;
using UnityEngine;

// Initiates a prototype chase; controller callbacks own chase completion.
public class FinalChaseHorrorEvent : HorrorEventTrigger
{
    [SerializeField] private GhostChaseController chaseController;
    [SerializeField] private GhostActor ghost;
    [SerializeField] private Transform apparitionSpawnPoint;
    [SerializeField] private float apparitionDuration = 1.5f;
    [SerializeField] private AudioSource chaseAudio;

    private void OnValidate()
    {
        apparitionDuration = float.IsNaN(apparitionDuration) || float.IsInfinity(apparitionDuration)
            ? 0f : Mathf.Max(0f, apparitionDuration);
    }

    protected override IEnumerator PlaySequence()
    {
        OnValidate();
        GhostActor shownGhost = null;
        if (ghost != null && apparitionSpawnPoint != null)
        {
            shownGhost = ghost;
            shownGhost.ShowAt(apparitionSpawnPoint);
            if (apparitionDuration > 0f)
                yield return new WaitForSeconds(apparitionDuration);
        }
        if (chaseAudio != null && chaseAudio.clip != null)
            chaseAudio.Play();

        GhostChaseController controller = chaseController;
        if (controller != null)
            controller.StartChase();
        if (controller == null || !controller.IsChasing)
        {
            if (shownGhost != null)
                shownGhost.Hide();
            yield break;
        }

        // Base completion must not report the chase ended at the start handoff.
        // The wrapper observes completion; it does not move or stop the ghost.
        while (controller != null && controller.IsChasing)
            yield return null;
    }
}
