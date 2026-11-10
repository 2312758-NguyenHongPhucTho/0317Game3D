using System.Collections;
using FpsHorrorKit;
using UnityEngine;

/// <summary>
/// One-shot scare sequence for Double_Door_2:
/// after the door has opened, Macon appears for 30 seconds and the rocking chair
/// starts using its existing rocking code after a 3-second delay.
/// </summary>
public sealed class DoorMaconSequence : MonoBehaviour
{
    [SerializeField] private DoorSystem door;
    [SerializeField] private GameObject macon;
    [SerializeField] private RockingChairLiftAndDrop rockingChair;
    [SerializeField] private float maconVisibleDuration = 30f;
    [SerializeField] private float rockingChairDelay = 3f;

    private bool sequenceStarted;

    private void Awake()
    {
        if (door == null)
            door = GetComponent<DoorSystem>();

        if (macon != null)
            macon.SetActive(false);

        if (rockingChair != null)
            rockingChair.triggerOnly = true;
    }

    private void Update()
    {
        if (!sequenceStarted && door != null && door.IsOpen && !door.IsMoving)
        {
            sequenceStarted = true;
            StartCoroutine(RunSequence());
        }
    }

    private IEnumerator RunSequence()
    {
        macon.SetActive(true);

        float delay = Mathf.Min(rockingChairDelay, maconVisibleDuration);
        yield return new WaitForSeconds(delay);

        if (rockingChair != null)
            rockingChair.TriggerRocking();

        yield return new WaitForSeconds(Mathf.Max(0f, maconVisibleDuration - delay));
        macon.SetActive(false);
    }
}
