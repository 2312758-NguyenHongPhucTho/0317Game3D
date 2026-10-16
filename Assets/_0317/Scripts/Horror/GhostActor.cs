using UnityEngine;

public class GhostActor : MonoBehaviour
{
    [SerializeField] private GameObject visualRoot;

    public bool IsVisible { get; private set; }

    private void Awake()
    {
        Hide();
    }

    public void ShowAt(Transform spawnPoint)
    {
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        Show();
    }

    public void Show()
    {
        if (visualRoot != null)
            visualRoot.SetActive(true);

        IsVisible = true;
    }

    public void Hide()
    {
        if (visualRoot != null)
            visualRoot.SetActive(false);

        IsVisible = false;
    }
}
