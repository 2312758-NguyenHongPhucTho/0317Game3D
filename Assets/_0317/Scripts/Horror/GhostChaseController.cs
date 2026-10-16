using UnityEngine;
using UnityEngine.Events;

// Fixed-route test controller; real route and progression are supplied later.
public class GhostChaseController : MonoBehaviour
{
    [SerializeField] private GhostActor ghost;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float waypointReachDistance = 0.15f;
    [SerializeField] private bool hideGhostWhenStopped = true;
    [SerializeField] private UnityEvent onChaseStarted = new UnityEvent();
    [SerializeField] private UnityEvent onChaseEnded = new UnityEvent();

    private bool isChasing;
    private int currentWaypointIndex;

    public bool IsChasing => isChasing;
    public int CurrentWaypointIndex => currentWaypointIndex;

    private static float SafeValue(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value) ? 0f : Mathf.Max(0f, value);
    }

    private void OnValidate()
    {
        moveSpeed = SafeValue(moveSpeed);
        rotationSpeed = SafeValue(rotationSpeed);
        waypointReachDistance = SafeValue(waypointReachDistance);
    }

    [ContextMenu("Test Start Chase")]
    public void StartChase()
    {
        if (isChasing || ghost == null || waypoints == null || waypoints.Length == 0)
            return;

        Transform firstValid = null;
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null) { firstValid = waypoint; break; }
        }
        if (firstValid == null)
            return;

        OnValidate();
        currentWaypointIndex = 0;
        ghost.ShowAt(firstValid);
        isChasing = true;
        onChaseStarted.Invoke();
    }

    private void Update()
    {
        if (!isChasing)
            return;
        if (ghost == null || waypoints == null)
        {
            StopChase();
            return;
        }

        while (currentWaypointIndex < waypoints.Length && waypoints[currentWaypointIndex] == null)
            currentWaypointIndex++;
        if (currentWaypointIndex >= waypoints.Length)
        {
            StopChase();
            return;
        }

        Transform actor = ghost.transform;
        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = target.position - actor.position;
        if (direction.sqrMagnitude > 0.000001f)
        {
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            actor.rotation = Quaternion.RotateTowards(actor.rotation, rotation, rotationSpeed * Time.deltaTime);
        }
        actor.position = Vector3.MoveTowards(actor.position, target.position, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(actor.position, target.position) <= waypointReachDistance)
        {
            actor.position = target.position;
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
                StopChase();
        }
    }

    [ContextMenu("Test Stop Chase")]
    public void StopChase()
    {
        if (!isChasing)
            return;
        isChasing = false;
        if (hideGhostWhenStopped && ghost != null)
            ghost.Hide();
        onChaseEnded.Invoke();
    }

    [ContextMenu("Reset Chase For Test")]
    public void ResetChaseForTest()
    {
        StopChase();
        currentWaypointIndex = 0;
    }
}
