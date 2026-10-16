using UnityEngine;

public class GhostPrototypeTest : MonoBehaviour
{
    [SerializeField] private GhostActor ghost;
    [SerializeField] private Transform spawnPoint;

    [ContextMenu("Test Show Ghost")]
    public void ShowGhost()
    {
        if (ghost != null)
            ghost.ShowAt(spawnPoint);
    }

    [ContextMenu("Test Hide Ghost")]
    public void HideGhost()
    {
        if (ghost != null)
            ghost.Hide();
    }
}
