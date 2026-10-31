using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class ExclusiveGhostAudio : MonoBehaviour
{
    [Header("Cai dat")]
    public bool playOnStart = true;

    private AudioSource ghostAudio;

    private readonly Dictionary<AudioSource, bool> previousMuteStates =
        new Dictionary<AudioSource, bool>();

    private Coroutine playRoutine;

    void Awake()
    {
        ghostAudio = GetComponent<AudioSource>();

        // Script se tu dieu khien viec phat
        ghostAudio.playOnAwake = false;
    }

    void Start()
    {
        if (playOnStart)
            PlayExclusive();
    }

    public void PlayExclusive()
    {
        if (playRoutine != null)
            return;

        playRoutine = StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        previousMuteStates.Clear();

        // Tim tat ca AudioSource dang ton tai
        AudioSource[] allAudioSources =
            FindObjectsByType<AudioSource>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        // Tat tieng cac AudioSource khac
        foreach (AudioSource audio in allAudioSources)
        {
            if (audio == null || audio == ghostAudio)
                continue;

            previousMuteStates[audio] = audio.mute;
            audio.mute = true;
        }

        // Phat giong ma
        ghostAudio.Play();

        // Doi den khi am thanh ket thuc
        while (ghostAudio != null && ghostAudio.isPlaying)
        {
            yield return null;
        }

        // Khoi phuc trang thai mute ban dau
        foreach (var item in previousMuteStates)
        {
            if (item.Key != null)
                item.Key.mute = item.Value;
        }

        previousMuteStates.Clear();
        playRoutine = null;
    }

    void OnDisable()
    {
        // Dam bao am thanh khac duoc khoi phuc
        foreach (var item in previousMuteStates)
        {
            if (item.Key != null)
                item.Key.mute = item.Value;
        }

        previousMuteStates.Clear();
        playRoutine = null;
    }
}