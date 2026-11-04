using FpsHorrorKit;
using UnityEngine;

/// <summary>
/// Opens a door once the player reaches the configured horizontal distance.
/// Attach this to the door prefab root; the moving leaf is found from its
/// DragToOpenSystem component when no explicit transform is assigned.
/// </summary>
public sealed class ProximityDoorOpener : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Transform player;
    [SerializeField] private string doorRootName = "Glass_Door_6";

    [Header("Proximity")]
    [SerializeField, Min(0.1f)] private float triggerDistance = 2f;

    [Header("Opening")]
    [SerializeField] private float openAngle = -105f;
    [SerializeField, Min(1f)] private float openingSpeed = 120f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool hasTriggered;

    private void Awake()
    {
        if (doorTransform == null)
        {
            GameObject doorRoot = GameObject.Find(doorRootName);
            Transform searchRoot = doorRoot != null ? doorRoot.transform : transform;
            DragToOpenSystem dragDoor = searchRoot.GetComponentInChildren<DragToOpenSystem>(true);
            doorTransform = dragDoor != null ? dragDoor.transform : searchRoot.Find("Door");
        }

        if (doorTransform == null)
        {
            Debug.LogWarning("ProximityDoorOpener could not find the moving door leaf.", this);
            enabled = false;
            return;
        }

        closedRotation = doorTransform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    private void Update()
    {
        if (!hasTriggered)
        {
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject == null)
                    return;

                player = playerObject.transform;
            }

            Vector3 playerPosition = player.position;
            Vector3 doorPosition = doorTransform.position;
            playerPosition.y = 0f;
            doorPosition.y = 0f;

            if (Vector3.Distance(playerPosition, doorPosition) <= triggerDistance)
                hasTriggered = true;
        }

        if (hasTriggered)
        {
            doorTransform.localRotation = Quaternion.RotateTowards(
                doorTransform.localRotation,
                openRotation,
                openingSpeed * Time.deltaTime);
        }
    }
}
