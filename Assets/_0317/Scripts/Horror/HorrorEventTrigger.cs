using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class HorrorEventTrigger : MonoBehaviour
{
    [SerializeField] private bool playOnce = true;
    [SerializeField] private bool armed = true;
    [SerializeField] private UnityEvent onEventStarted = new UnityEvent();
    [SerializeField] private UnityEvent onEventCompleted = new UnityEvent();

    private bool hasPlayed;
    private bool isPlaying;

    public bool HasPlayed => hasPlayed;
    public bool IsPlaying => isPlaying;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            TryPlay();
    }

    public void TryPlay()
    {
        if (!armed || isPlaying || (playOnce && hasPlayed))
            return;

        StartCoroutine(EventSequence());
    }

    private IEnumerator EventSequence()
    {
        isPlaying = true;
        if (playOnce)
            hasPlayed = true;

        onEventStarted.Invoke();
        yield return PlaySequence();
        onEventCompleted.Invoke();
        isPlaying = false;
    }

    protected virtual IEnumerator PlaySequence()
    {
        yield break;
    }

    public void Arm()
    {
        armed = true;
    }

    public void Disarm()
    {
        armed = false;
    }

    [ContextMenu("Reset Event For Test")]
    public void ResetEventForTest()
    {
        if (!isPlaying)
            hasPlayed = false;
    }

    [ContextMenu("Test Play Event")]
    public void TestPlayEvent()
    {
        TryPlay();
    }
}
