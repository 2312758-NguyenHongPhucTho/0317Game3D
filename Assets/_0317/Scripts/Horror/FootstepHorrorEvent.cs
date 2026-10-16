using System.Collections;
using UnityEngine;

public class FootstepHorrorEvent : HorrorEventTrigger
{
    [SerializeField] private AudioSource footstepAudio;

    protected override IEnumerator PlaySequence()
    {
        if (footstepAudio == null || footstepAudio.clip == null)
            yield break;

        footstepAudio.Play();
        while (footstepAudio != null && footstepAudio.isPlaying)
            yield return null;
    }
}
