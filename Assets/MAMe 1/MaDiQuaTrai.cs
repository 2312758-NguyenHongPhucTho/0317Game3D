
using UnityEngine;
using System.Collections;

public class MaDiQuaTrai : MonoBehaviour
{
    [Header("PLAYER")]
    public Transform player;
    public float triggerDistance = 5f;

    [Header("GHOST")]
    public Transform ghostRoot;
    public Animation ghostAnimation;

    [Header("MIXAMO ANIMATION - LEGACY")]
    public AnimationClip walkClip;
    public AnimationClip idleClip;

    [Header("MOVEMENT")]
    public Transform walkTarget;
    public float moveSpeed = 1.2f;
    public float walkDistance = 3f;
    public float stopDistance = 1f;
    public float arrivalDistance = 0.15f;
    public float turnSpeed = 6f;
    public float animationSpeed = 1f;
    public float maxWalkTime = 30f;

    [Header("WALK STRAFE LEFT")]
    public bool useStrafeLeftAnimation = false;

    [Header("TRANSITION")]
    public float crossFadeTime = 0.25f;
    public bool hideAfterWalking = false;

    private bool hasStarted = false;

    private const string WALK = "Walk";
    private const string IDLE = "Idle";

    private void Awake()
    {
        if (ghostRoot == null)
            ghostRoot = transform;

        if (ghostAnimation == null)
            ghostAnimation =
                GetComponentInChildren<Animation>(true);

        if (ghostAnimation == null)
        {
            Debug.LogError("Khong tim thay Animation component!");
            enabled = false;
            return;
        }

        ghostAnimation.playAutomatically = false;

        // Tat Animator neu dang su dung Legacy Animation
        Animator[] animators =
            ghostRoot.GetComponentsInChildren<Animator>(true);

        foreach (Animator animator in animators)
            animator.enabled = false;

        AddClip(walkClip, WALK, WrapMode.Loop);
        AddClip(idleClip, IDLE, WrapMode.Loop);
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject obj =
                GameObject.FindGameObjectWithTag("Player");

            if (obj != null)
                player = obj.transform;
        }

        if (idleClip != null &&
            ghostAnimation[IDLE] != null)
        {
            ghostAnimation.Play(IDLE);
        }
    }

    private void AddClip(
        AnimationClip clip,
        string clipName,
        WrapMode mode)
    {
        if (clip == null)
            return;

        if (!clip.legacy)
        {
            Debug.LogError(
                clip.name + " phai duoc import dang Legacy!"
            );
            return;
        }

        if (ghostAnimation.GetClip(clipName) != null)
            ghostAnimation.RemoveClip(clipName);

        ghostAnimation.AddClip(clip, clipName);
        ghostAnimation[clipName].wrapMode = mode;
    }

    private void Update()
    {
        if (hasStarted || player == null)
            return;

        Vector3 playerPos = player.position;
        Vector3 ghostPos = ghostRoot.position;

        playerPos.y = 0f;
        ghostPos.y = 0f;

        float distance =
            Vector3.Distance(playerPos, ghostPos);

        if (distance <= triggerDistance)
        {
            hasStarted = true;
            StartCoroutine(WalkSequence());
        }
    }

    private IEnumerator WalkSequence()
    {
        if (ghostAnimation[WALK] == null)
        {
            Debug.LogError("Chua gan Walk Animation!");
            yield break;
        }

        AnimationState walk = ghostAnimation[WALK];

        walk.time = 0f;
        walk.speed = animationSpeed;
        walk.wrapMode = WrapMode.Loop;

        ghostAnimation.CrossFade(
            WALK,
            crossFadeTime
        );

        // Luu vi tri va huong ban dau
        Vector3 startPosition = ghostRoot.position;
        Quaternion startRotation = ghostRoot.rotation;

        Vector3 fixedTarget;

        if (useStrafeLeftAnimation)
        {
            // Di ngang sang trai theo animation Mixamo
            fixedTarget =
                startPosition - startRotation * Vector3.right
                * walkDistance;
        }
        else if (walkTarget != null)
        {
            // Di den diem duoc chi dinh
            fixedTarget = walkTarget.position;
        }
        else
        {
            // Neu khong co target thi di thang
            fixedTarget =
                startPosition + startRotation * Vector3.forward
                * walkDistance;
        }

        float timer = 0f;

        while (timer < maxWalkTime)
        {
            Vector3 target = followPlayerTarget()
                ? player.position
                : fixedTarget;

            target.y = ghostRoot.position.y;

            Vector3 direction =
                target - ghostRoot.position;

            direction.y = 0f;

            float stopRange = followPlayerTarget()
                ? stopDistance
                : arrivalDistance;

            if (direction.magnitude <= stopRange)
                break;

            // Quay mat theo huong di neu khong strafing
            if (!useStrafeLeftAnimation)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction.normalized);

                ghostRoot.rotation = Quaternion.Slerp(
                    ghostRoot.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime
                );
            }

            // Di chuyen root cua ma
            ghostRoot.position = Vector3.MoveTowards(
                ghostRoot.position,
                new Vector3(
                    target.x,
                    ghostRoot.position.y,
                    target.z
                ),
                moveSpeed * Time.deltaTime
            );

            timer += Time.deltaTime;

            yield return null;
        }

        // Dung lai sau khi di den dich
        if (ghostAnimation[IDLE] != null)
        {
            ghostAnimation.CrossFade(
                IDLE,
                crossFadeTime
            );
        }
        else
        {
            ghostAnimation.Stop();
        }

        if (hideAfterWalking)
            ghostRoot.gameObject.SetActive(false);
    }

    private bool followPlayerTarget()
    {
        return false;
    }
}
