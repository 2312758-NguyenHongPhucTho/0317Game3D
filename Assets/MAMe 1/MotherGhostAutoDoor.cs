using System.Collections;
using FpsHorrorKit;
using UnityEngine;

public class MotherGhostMovement : MonoBehaviour
{
    [Header("PLAYER")]
    public Transform player;
    public float walkTriggerDistance = 2.5f;

    [Header("DAMAGE")]
    [Tooltip("Component PlayerHealth0317 trên Player. Nếu trống, script tự tìm từ Player Transform.")]
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

    [Header("TRANSITION")]
    public float transitionTime = 0.25f;
    public bool disappearAfterWalking = false;

    private bool isWalkingToPlayer;
    private bool hasDealtDamage;

    private const string POINT = "Point";
    private const string WALK = "Walk";
    private const string IDLE = "Idle";

    private void Awake()
    {
        if (ghostObject == null)
        {
            Debug.LogError("[03:17] Chưa gán Ghost Object!", this);
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

        if (ghostAnimation == null)
        {
            Debug.LogError("[03:17] Không tìm thấy Animation component!", this);
            enabled = false;
            return;
        }

        FindPlayerHealth();
        ghostAnimation.playAutomatically = false;

        // Không dùng Animator song song với các clip Legacy trên cùng nhân vật.
        Animator[] animators = ghostObject.GetComponentsInChildren<Animator>(true);
        foreach (Animator animator in animators)
            animator.enabled = false;

        AddClip(pointingClip, POINT, WrapMode.Loop);
        AddClip(walkingClip, WALK, WrapMode.Loop);
        AddClip(idleClip, IDLE, WrapMode.Loop);

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
            Debug.LogWarning("[03:17] Chưa gắn Pointing Clip!", this);
        }
    }

    private void AddClip(AnimationClip clip, string clipName, WrapMode wrapMode)
    {
        if (clip == null) return;

        if (!clip.legacy)
        {
            Debug.LogError(clip.name + " phải được import dạng Legacy!", this);
            return;
        }

        if (ghostAnimation.GetClip(clipName) != null)
            ghostAnimation.RemoveClip(clipName);

        ghostAnimation.AddClip(clip, clipName);
        ghostAnimation[clipName].wrapMode = wrapMode;
    }

    private void Update()
    {
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
            Debug.LogError("[03:17] Chưa gắn Player hoặc Walking Clip!", this);
            yield break;
        }

        AnimationState walkAnim = ghostAnimation[WALK];
        walkAnim.time = 0f;
        walkAnim.speed = 1f;
        ghostAnimation.CrossFade(WALK, transitionTime);

        float timer = 0f;
        bool reachedPlayer = false;
        while (timer < maxWalkTime)
        {
            if (player == null) break;

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
                ghostRoot.rotation = Quaternion.Slerp(ghostRoot.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            ghostRoot.position = Vector3.MoveTowards(
                ghostRoot.position,
                new Vector3(targetPosition.x, ghostRoot.position.y, targetPosition.z),
                walkSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        // Chỉ gây sát thương khi đã thực sự đi tới khoảng cách dừng; tối đa một lần.
        if (reachedPlayer && !hasDealtDamage)
        {
            hasDealtDamage = true;
            FindPlayerHealth();
            if (playerHealth != null)
            {
                if (playerHealth.TakeTrapDamage(damageAmount))
                    Debug.Log($"[03:17] Ma mẹ gây {damageAmount} sát thương khi đến gần Player.", this);
            }
            else
            {
                Debug.LogWarning("[03:17] Không tìm thấy PlayerHealth0317: ma mẹ không trừ HP.", this);
            }
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
}
