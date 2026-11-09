using System.Collections;
using FpsHorrorKit;
using UnityEngine;

public class MotherGhostMovement : MonoBehaviour
{
    [Header("PLAYER")]
    public Transform player;
    public float walkTriggerDistance = 2.5f;

    [Header("DAMAGE")]
    [Tooltip("PlayerHealth0317 tren Player. Neu trong, script tu tim tu Player Transform.")]
    public PlayerHealth0317 playerHealth;
    [Min(1)] public int damageAmount = 10;

    [Header("GHOST")]
    public GameObject ghostObject;
    public Transform ghostRoot;
    public Animation ghostAnimation;

    [Header("MIXAMO ANIMATIONS - LEGACY")]
    public AnimationClip pointingClip;
    public AnimationClip walkingClip;
    public AnimationClip idleClip;

    [Header("POINTING")]
    public float pointingSpeed = 1f;

    [Header("WALKING TOWARDS PLAYER")]
    public float walkSpeed = 1.2f;
    public float stopDistance = 1.2f;
    public float turnSpeed = 5f;
    public float maxWalkTime = 30f;

    [Header("MOVEMENT MODE")]
    [Tooltip("Bat: ma duoi theo Player. Tat: ma di thang theo huong forward ban dau.")]
    public bool followPlayer = true;
    [Tooltip("So met ma di thang khi Follow Player = false.")]
    [Min(0.1f)] public float straightWalkDistance = 6f;

    [Header("GHOST AUDIO")]
    [Tooltip("AudioSource de phat tieng ma. Nen gan AudioSource rieng, khong dung AudioSource nhac phong.")]
    public AudioSource ghostAudioSource;
    [Tooltip("Phat 1 lan khi ma xuat hien / bat dau animation chi tay.")]
    public AudioClip appearanceSound;
    [Tooltip("Phat 1 lan khi ma bat dau di hoac khi ma den sat Player.")]
    public AudioClip scareSound;
    [Range(0f, 1f)] public float appearanceVolume = 1f;
    [Range(0f, 1f)] public float scareVolume = 1f;
    [Tooltip("Bat: phat tieng hu ngay khi ma bat dau di. Tat: phat khi ma den gan Player trong stopDistance.")]
    public bool scareWhenWalkingStarts = true;

    [Header("TRANSITION")]
    public float transitionTime = 0.25f;
    public bool disappearAfterWalking = false;

    private bool isWalkingToPlayer;
    private bool hasDealtDamage;
    private bool hasPlayedScareSound;

    private const string POINT = "Point";
    private const string WALK = "Walk";
    private const string IDLE = "Idle";

    private void Awake()
    {
        if (ghostObject == null)
        {
            Debug.LogError("[03:17] Chua gan Ghost Object!", this);
            enabled = false;
            return;
        }

        if (ghostRoot == null)
            ghostRoot = ghostObject.transform;

        if (ghostAnimation == null)
        {
            ghostAnimation = ghostObject.GetComponentInChildren<Animation>(true);
            if (ghostAnimation == null)
                ghostAnimation = ghostObject.AddComponent<Animation>();
        }

        FindPlayerHealth();
        ghostAnimation.playAutomatically = false;

        // Khong dung Animator song song voi Animation Legacy tren cung nhan vat.
        Animator[] animators = ghostObject.GetComponentsInChildren<Animator>(true);
        foreach (Animator animator in animators)
            animator.enabled = false;

        AddClip(pointingClip, POINT, WrapMode.Loop);
        AddClip(walkingClip, WALK, WrapMode.Loop);
        AddClip(idleClip, IDLE, WrapMode.Loop);

        // Chi tao AudioSource neu chua duoc gan trong Inspector.
        if (ghostAudioSource == null)
        {
            ghostAudioSource = gameObject.AddComponent<AudioSource>();
            ghostAudioSource.playOnAwake = false;
            ghostAudioSource.loop = false;
            ghostAudioSource.spatialBlend = 1f; // 3D: tieng ma phat ra tu vi tri object.
            ghostAudioSource.minDistance = 1f;
            ghostAudioSource.maxDistance = 18f;
        }

        ghostObject.SetActive(true);
    }

    private void FindPlayerHealth()
    {
        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth0317>();
            if (playerHealth == null)
                playerHealth = player.GetComponentInParent<PlayerHealth0317>();
        }
    }

    private void Start()
    {
        if (ghostAnimation[POINT] != null)
        {
            ghostAnimation[POINT].speed = pointingSpeed;
            ghostAnimation.Play(POINT);
        }
        else
        {
            Debug.LogWarning("[03:17] Chua gan Pointing Clip!", this);
        }

        // Tieng xuat hien phat 1 lan khi script bat dau hoat dong.
        PlaySound(appearanceSound, appearanceVolume);
    }

    private void AddClip(AnimationClip clip, string clipName, WrapMode wrapMode)
    {
        if (clip == null) return;

        if (!clip.legacy)
        {
            Debug.LogError(clip.name + " phai duoc import dang Legacy!", this);
            return;
        }

        if (ghostAnimation.GetClip(clipName) != null)
            ghostAnimation.RemoveClip(clipName);

        ghostAnimation.AddClip(clip, clipName);
        ghostAnimation[clipName].wrapMode = wrapMode;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return; // Khong bat dau hu khi game dang Pause.
        if (isWalkingToPlayer || player == null || ghostRoot == null) return;

        Vector3 flatPlayerPosition = player.position;
        Vector3 flatGhostPosition = ghostRoot.position;
        flatPlayerPosition.y = 0f;
        flatGhostPosition.y = 0f;

        if (Vector3.Distance(flatPlayerPosition, flatGhostPosition) <= walkTriggerDistance)
        {
            isWalkingToPlayer = true;
            StartCoroutine(WalkTowardsPlayerRoutine());
        }
    }

    private IEnumerator WalkTowardsPlayerRoutine()
    {
        if (player == null || ghostAnimation[WALK] == null)
        {
            Debug.LogError("[03:17] Chua gan Player hoac Walking Clip!", this);
            isWalkingToPlayer = false;
            yield break;
        }

        AnimationState walkAnim = ghostAnimation[WALK];
        walkAnim.time = 0f;
        walkAnim.speed = 1f;
        ghostAnimation.CrossFade(WALK, transitionTime);

        if (scareWhenWalkingStarts)
            PlayScareSoundOnce();

        bool chasePlayer = followPlayer;
        Vector3 straightDirection = ghostRoot.forward;
        straightDirection.y = 0f;
        if (straightDirection.sqrMagnitude < 0.0001f)
            straightDirection = Vector3.forward;
        straightDirection.Normalize();

        float straightDistanceWalked = 0f;
        float timer = 0f;
        bool reachedPlayer = false;

        while (timer < maxWalkTime)
        {
            if (player == null) break;

            if (chasePlayer)
            {
                Vector3 targetPosition = player.position;
                Vector3 direction = targetPosition - ghostRoot.position;
                direction.y = 0f;

                if (direction.magnitude <= stopDistance)
                {
                    reachedPlayer = true;
                    break;
                }

                if (direction.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                    ghostRoot.rotation = Quaternion.Slerp(
                        ghostRoot.rotation, targetRotation, turnSpeed * Time.deltaTime);
                }

                ghostRoot.position = Vector3.MoveTowards(
                    ghostRoot.position,
                    new Vector3(targetPosition.x, ghostRoot.position.y, targetPosition.z),
                    walkSpeed * Time.deltaTime);
            }
            else
            {
                if (straightDistanceWalked >= straightWalkDistance)
                    break;

                float step = Mathf.Min(
                    Mathf.Max(0f, walkSpeed) * Time.deltaTime,
                    straightWalkDistance - straightDistanceWalked);

                ghostRoot.position += straightDirection * step;
                straightDistanceWalked += step;

                // Neu di thang ma van cham gan Player, van tru HP va phat tieng hu.
                Vector3 difference = player.position - ghostRoot.position;
                difference.y = 0f;
                if (difference.magnitude <= stopDistance)
                {
                    if (!scareWhenWalkingStarts)
                        PlayScareSoundOnce();
                    DealDamageOnce();
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (chasePlayer && reachedPlayer)
        {
            if (!scareWhenWalkingStarts)
                PlayScareSoundOnce();
            DealDamageOnce();
        }

        if (disappearAfterWalking)
        {
            ghostObject.SetActive(false);
        }
        else if (ghostAnimation[IDLE] != null)
        {
            ghostAnimation.CrossFade(IDLE, transitionTime);
        }
        else
        {
            walkAnim.speed = 0f;
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (ghostAudioSource == null || clip == null) return;
        ghostAudioSource.PlayOneShot(clip, volume);
    }

    private void PlayScareSoundOnce()
    {
        if (hasPlayedScareSound) return;
        if (scareSound == null) return;

        hasPlayedScareSound = true;
        PlaySound(scareSound, scareVolume);
    }

    private void DealDamageOnce()
    {
        if (hasDealtDamage) return;
        hasDealtDamage = true;
        FindPlayerHealth();

        if (playerHealth != null)
        {
            if (playerHealth.TakeTrapDamage(damageAmount))
                Debug.Log($"[03:17] Ma me gay {damageAmount} sat thuong khi den gan Player.", this);
        }
        else
        {
            Debug.LogWarning("[03:17] Khong tim thay PlayerHealth0317: ma me khong tru HP.", this);
        }
    }
}
