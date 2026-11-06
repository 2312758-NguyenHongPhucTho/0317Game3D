using UnityEngine;

/// <summary>
/// Shows the configured ghost once the player enters the radius around this object.
/// Attach this to StairsRailing_1 and assign the Opening ghost.
/// </summary>
public sealed class StairsRailingGhostTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject openingGhost;

    [Header("Trigger")]
    [SerializeField, Min(0.1f)] private float triggerDistance = 3f;
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (openingGhost == null)
        {
            Debug.LogError("[03:17] StairsRailingGhostTrigger needs the Opening ghost reference.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (hasTriggered && triggerOnlyOnce)
            return;

        if (player == null || openingGhost == null)
            return;

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;

        if (offset.sqrMagnitude > triggerDistance * triggerDistance)
            return;

        hasTriggered = true;
        openingGhost.SetActive(true);
        Debug.Log("[03:17] Player reached StairsRailing_1: Opening ghost activated.", this);

        if (!triggerOnlyOnce)
            hasTriggered = false;
    }
}