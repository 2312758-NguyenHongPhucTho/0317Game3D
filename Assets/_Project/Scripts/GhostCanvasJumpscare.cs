using System.Collections;
using FpsHorrorKit;
using UnityEngine;
using UnityEngine.UI;

// Dat script tren mot GameObject rong o vi tri muon kich hoat jumpscare.
// Sau khi nguoi choi da tra radio (nhac da bat), den gan se tat nhac,
// doi mot luc, hien ma + phat tieng het, va tru HP mot lan.
public class GhostAutoCanvasJumpscare : MonoBehaviour
{
    [Header("PLAYER & GHOST")]
    public GameObject playerObject;
    public Sprite ghostSprite;
    public PlayerHealth0317 playerHealth;
    [Min(1)] public int damageAmount = 10;

    [Header("JUMPSCARE")]
    [Min(0.1f)] public float triggerDistance = 2.5f;
    [Min(0f)] public float delayBeforeShow = 2f;
    [Min(0.1f)] public float showDuration = 3f;

    [Header("RADIO")]
    public RadioInspectMusic0317 radioMusic;
    public AudioSource amThanhPhongAn;

    [Header("GHOST SCREAM")]
    public AudioClip screamClip;
    [Range(0f, 1f)] public float screamVolume = 1f;

    private bool hasTriggered;
    private GameObject canvasObject;
    private AudioSource screamSource;
    private Coroutine sequence;

    private void Start()
    {
        if (playerObject != null && playerHealth == null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth0317>();
            if (playerHealth == null)
                playerHealth = playerObject.GetComponentInParent<PlayerHealth0317>();
        }

        // AudioSource rieng cho tieng het, khong dung AudioSource nhac phong an.
        screamSource = gameObject.AddComponent<AudioSource>();
        screamSource.playOnAwake = false;
        screamSource.loop = false;
        screamSource.spatialBlend = 0f; // 2D, nghe ro khi hien canvas.

        if (ghostSprite == null)
        {
            Debug.LogError("[03:17] Chua gan Ghost Sprite.", this);
            return;
        }

        CreateCanvas();

        if (radioMusic == null)
            Debug.LogWarning("[03:17] Chua gan Radio Music tren radio_low.", this);
        if (amThanhPhongAn == null)
            Debug.LogWarning("[03:17] Chua gan AudioSource AmThanhPhongAn.", this);
    }

    private void Update()
    {
        if (hasTriggered || canvasObject == null || playerObject == null ||
            radioMusic == null || !radioMusic.HasStarted || Time.timeScale <= 0f)
            return;

        // Chi so khoang cach tren mat phang, tranh lech do chieu cao camera.
        Vector3 offset = playerObject.transform.position - transform.position;
        offset.y = 0f;
        if (offset.sqrMagnitude > triggerDistance * triggerDistance)
            return;

        hasTriggered = true;
        sequence = StartCoroutine(JumpscareSequence());
    }

    private IEnumerator JumpscareSequence()
    {
        // Nhac dung ngay khi nguoi choi den gan.
        if (amThanhPhongAn != null)
            amThanhPhongAn.Stop();
        else
            Debug.LogWarning("[03:17] Khong the tat nhac: chua gan AmThanhPhongAn.", this);

        yield return new WaitForSeconds(delayBeforeShow);

        if (canvasObject != null)
        {
            canvasObject.SetActive(true);

            // Tieng ma het phat DUNG LUC canvas xuat hien.
            if (screamClip != null && screamSource != null)
                screamSource.PlayOneShot(screamClip, screamVolume);

            if (playerHealth != null)
                playerHealth.TakeTrapDamage(damageAmount);
            else
                Debug.LogWarning("[03:17] Chua gan Player Health, khong tru HP.", this);
        }

        yield return new WaitForSeconds(showDuration);
        if (canvasObject != null)
            canvasObject.SetActive(false);
        sequence = null;
    }

    private void CreateCanvas()
    {
        canvasObject = new GameObject("AutoJumpscareCanvas", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject imageObject = new GameObject("GhostImage", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);

        Image image = imageObject.GetComponent<Image>();
        image.sprite = ghostSprite;
        image.raycastTarget = false;

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        canvasObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (sequence != null)
        {
            StopCoroutine(sequence);
            sequence = null;
        }
        if (canvasObject != null)
            canvasObject.SetActive(false);
        if (screamSource != null)
            screamSource.Stop();
    }

    private void OnDestroy()
    {
        if (canvasObject != null)
            Destroy(canvasObject);
    }
}
