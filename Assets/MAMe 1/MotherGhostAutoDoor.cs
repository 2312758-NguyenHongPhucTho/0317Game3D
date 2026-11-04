using UnityEngine;
using System.Collections;

public class MotherGhostMovement : MonoBehaviour
{
    [Header("PLAYER")]
    public Transform player;
    public float walkTriggerDistance = 2.5f;

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
    public float walkSpeed = 1.2f;        // Tốc độ đi bộ từ từ về phía player
    public float stopDistance = 1.2f;     // Khoảng cách dừng lại khi tiến đến gần player
    public float turnSpeed = 5f;          // Tốc độ xoay người nhìn player
    public float maxWalkTime = 30f;       // Thời gian tối đa đi bộ

    [Header("TRANSITION")]
    public float transitionTime = 0.25f;
    public bool disappearAfterWalking = false;

    private bool isWalkingToPlayer = false;

    private const string POINT = "Point";
    private const string WALK = "Walk";
    private const string IDLE = "Idle";

    private void Awake()
    {
        if (ghostRoot == null && ghostObject != null)
            ghostRoot = ghostObject.transform;

        if (ghostAnimation == null && ghostObject != null)
        {
            ghostAnimation = ghostObject.GetComponentInChildren<Animation>(true);

            if (ghostAnimation == null)
                ghostAnimation = ghostObject.AddComponent<Animation>();
        }

        if (ghostAnimation == null)
        {
            Debug.LogError("Không tìm thấy Animation component!", this);
            enabled = false;
            return;
        }

        ghostAnimation.playAutomatically = false;

        // Tắt Animator để tránh xung đột với Legacy Animation
        Animator[] animators = ghostObject.GetComponentsInChildren<Animator>(true);
        foreach (Animator animator in animators)
            animator.enabled = false;

        AddClip(pointingClip, POINT, WrapMode.Loop);
        AddClip(walkingClip, WALK, WrapMode.Loop);
        AddClip(idleClip, IDLE, WrapMode.Loop);

        if (ghostObject != null)
            ghostObject.SetActive(true);
    }

    private void Start()
    {
        // Ban đầu đứng tại chỗ và chỉ tay liên tục
        if (ghostAnimation[POINT] != null)
        {
            ghostAnimation[POINT].speed = pointingSpeed;
            ghostAnimation.Play(POINT);
        }
        else
        {
            Debug.LogError("Chưa gắn Pointing Clip!", this);
        }
    }

    private void AddClip(AnimationClip clip, string clipName, WrapMode wrapMode)
    {
        if (clip == null)
            return;

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
        if (isWalkingToPlayer || player == null || ghostRoot == null)
            return;

        Vector3 playerPosition = player.position;
        Vector3 ghostPosition = ghostRoot.position;

        // Chỉ tính khoảng cách trên mặt đất (mặt phẳng XZ)
        playerPosition.y = 0f;
        ghostPosition.y = 0f;

        float distance = Vector3.Distance(playerPosition, ghostPosition);

        // Người chơi lại gần -> bắt đầu đi bộ về phía player
        if (distance <= walkTriggerDistance)
        {
            isWalkingToPlayer = true;
            StartCoroutine(WalkTowardsPlayerRoutine());
        }
    }

    private IEnumerator WalkTowardsPlayerRoutine()
    {
        if (player == null || ghostAnimation[WALK] == null)
        {
            Debug.LogError("Chưa gắn Player hoặc Walking Clip!", this);
            yield break;
        }

        // Chuyển từ trạng thái chỉ tay sang đi bộ
        AnimationState walkAnim = ghostAnimation[WALK];
        walkAnim.time = 0f;
        walkAnim.speed = 1f;

        ghostAnimation.CrossFade(WALK, transitionTime);

        float timer = 0f;

        // Liên tục tiến về phía Player đang di chuyển
        while (timer < maxWalkTime)
        {
            if (player == null) break;

            Vector3 targetPosition = player.position;
            Vector3 direction = targetPosition - ghostRoot.position;
            direction.y = 0f; // Giữ hướng trên mặt đất

            // Nếu đã đến gần player trong khoảng cho phép thì dừng lại
            if (direction.magnitude <= stopDistance)
                break;

            // Xoay mặt từ từ về phía Player
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            ghostRoot.rotation = Quaternion.Slerp(
                ghostRoot.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );

            // Di chuyển đi bộ từ từ về hướng Player
            ghostRoot.position = Vector3.MoveTowards(
                ghostRoot.position,
                new Vector3(targetPosition.x, ghostRoot.position.y, targetPosition.z),
                walkSpeed * Time.deltaTime
            );

            timer += Time.deltaTime;
            yield return null;
        }

        // Khi đi tới nơi / sát player: Chuyển sang Idle hoặc biến mất
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