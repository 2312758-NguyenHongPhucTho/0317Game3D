using FpsHorrorKit;
using UnityEngine;

/// <summary>
/// Phat nhac cua AmThanhPhongAn sau khi InspectSystem da tra radio ve cho cu.
/// Gan script nay tren CHINH GameObject co InspectSystem (radio_low).
/// </summary>
[RequireComponent(typeof(InspectSystem))]
public sealed class RadioInspectMusic0317 : MonoBehaviour
{
    [Header("AUDIO IN THE ROOM")]
    [Tooltip("Keo AudioSource cua AmThanhPhongAn vao day.")]
    [SerializeField] private AudioSource amThanhPhongAn;

    [Tooltip("Chi bat nhac lan dau tien khi tra radio.")]
    [SerializeField] private bool playOnlyOnce = true;

    public bool HasStarted { get; private set; }

    private InspectSystem inspectSystem;

    private void Awake()
    {
        inspectSystem = GetComponent<InspectSystem>();
    }

    private void OnEnable()
    {
        if (inspectSystem == null) inspectSystem = GetComponent<InspectSystem>();
        if (inspectSystem != null)
            inspectSystem.InspectionReturned += PlayAfterRadioReturned;
    }

    private void OnDisable()
    {
        if (inspectSystem != null)
            inspectSystem.InspectionReturned -= PlayAfterRadioReturned;
    }

    private void PlayAfterRadioReturned()
    {
        if (playOnlyOnce && HasStarted) return;
        if (amThanhPhongAn == null)
        {
            Debug.LogWarning("[03:17] Radio chua gan AudioSource AmThanhPhongAn.", this);
            return;
        }
        if (!amThanhPhongAn.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[03:17] GameObject AmThanhPhongAn dang inactive.", this);
            return;
        }
        if (amThanhPhongAn.clip == null)
        {
            Debug.LogWarning("[03:17] AmThanhPhongAn chua gan AudioClip.", this);
            return;
        }

        if (!amThanhPhongAn.enabled) amThanhPhongAn.enabled = true;
        amThanhPhongAn.loop = true;
        if (!amThanhPhongAn.isPlaying) amThanhPhongAn.Play();
        HasStarted = true;
        Debug.Log("[03:17] Da tra radio, bat dau phat AmThanhPhongAn.", this);
    }
}
