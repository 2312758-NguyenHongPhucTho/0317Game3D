using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DoorSlamHorrorEvent : HorrorEventTrigger
{
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 closedLocalEulerAngles;
    [SerializeField] private float slamDuration = 0.15f;
    [SerializeField] private AudioSource slamAudio;
    [SerializeField] private UnityEvent onDoorSlam = new UnityEvent();

    private void OnValidate()
    {
        ValidateDuration();
    }

    private void ValidateDuration()
    {
        slamDuration = float.IsNaN(slamDuration) || float.IsInfinity(slamDuration)
            ? 0f : Mathf.Max(0f, slamDuration);
    }

    protected override IEnumerator PlaySequence()
    {
        if (doorTransform == null)
            yield break;

        ValidateDuration();
        Transform door = doorTransform;
        Quaternion initialRotation = door.localRotation;
        Quaternion closedRotation = Quaternion.Euler(closedLocalEulerAngles);
        float duration = slamDuration;

        if (slamAudio != null && slamAudio.clip != null)
            slamAudio.Play();
        onDoorSlam.Invoke();

        float elapsed = 0f;
        while (elapsed < duration && door != null)
        {
            door.localRotation = Quaternion.Slerp(initialRotation, closedRotation, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (door != null)
            door.localRotation = closedRotation;
    }
}
