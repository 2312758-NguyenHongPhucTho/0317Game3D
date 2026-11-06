using UnityEngine;

/// <summary>
/// Plays the configured Animator state once when the player gets close.
/// </summary>
[RequireComponent(typeof(Animator))]
public sealed class GhostProximityLuot : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField, Min(0.1f)] private float triggerDistance = 3f;
    [SerializeField] private string stateName = "Luot";
    [Header("Sideways movement")]
    [SerializeField, Min(0.1f)] private float strafeSpeed = 2.5f;
    [SerializeField, Min(0.1f)] private float strafeDistance = 4f;
    [SerializeField] private Animator animator;

    private bool hasTriggered;
    private Vector3 startPosition;
    private Vector3 strafeDirection;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogWarning("GhostProximityLuot requires an Animator.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (player == null || animator == null)
            return;

        if (!hasTriggered)
        {
            Vector3 offset = player.position - transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude > triggerDistance * triggerDistance)
                return;

            animator.Play(stateName, 0, 0f);
            startPosition = transform.position;
            strafeDirection = -transform.right;
            hasTriggered = true;
        }

        Vector3 travelled = transform.position - startPosition;
        travelled.y = 0f;
        float remainingDistance = strafeDistance - travelled.magnitude;

        if (remainingDistance <= 0f)
            return;

        transform.position += strafeDirection * Mathf.Min(
            strafeSpeed * Time.deltaTime,
            remainingDistance
        );
    }
}
