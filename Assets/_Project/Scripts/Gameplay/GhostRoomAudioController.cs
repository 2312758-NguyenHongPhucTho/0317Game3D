using UnityEngine;

/// <summary>
/// Starts the ghost-room ambience only after the player approaches it, then
/// permanently stops it when the player reaches the configured exit floor.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public sealed class GhostRoomAudioController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform stopTarget;
    [SerializeField, Min(0.1f)] private float startDistance = 4f;
    [SerializeField, Min(0.1f)] private float stopDistance = 1.5f;
    [SerializeField] private AudioSource audioSource;

    private bool permanentlyStopped;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning("GhostRoomAudioController requires an AudioSource.", this);
            enabled = false;
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.Stop();
    }

    private void Update()
    {
        if (permanentlyStopped || player == null)
            return;

        if (stopTarget != null && IsWithinDistance(stopTarget, stopDistance))
        {
            audioSource.Stop();
            permanentlyStopped = true;
            return;
        }

        if (!audioSource.isPlaying && IsWithinDistance(transform, startDistance))
            audioSource.Play();
    }

    private bool IsWithinDistance(Transform target, float distance)
    {
        Vector3 offset = player.position - target.position;
        offset.y = 0f;
        return offset.sqrMagnitude <= distance * distance;
    }
}
