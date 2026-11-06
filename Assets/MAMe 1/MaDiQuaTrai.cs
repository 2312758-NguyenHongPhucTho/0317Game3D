
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

    [Header("MIXAMO ANIMATION")]
    public AnimationClip walkStrafeLeftClip;
    public AnimationClip idleClip;

    [Header("DI CHUYEN SANG TRAI")]
    public float walkDistance = 3f;
    public float moveSpeed = 1.2f;
    public float animationSpeed = 1f;

    [Header("CHUYEN ANIMATION")]
    public float transitionTime = 0.25f;

    [Header("SAU KHI DI CHUYEN")]
    public bool hideAfterWalking = false;

    private bool hasTriggered = false;
    private bool isWalking = false;

    private const string WALK = "WalkStrafeLeft";
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

        // Tat Animator de tranh xung dot voi Legacy Animation
        Animator[] animators =
            ghostRoot.GetComponentsInChildren<Animator>(true);

        foreach (Animator animator in animators)
            animator.enabled = false;

        RegisterClip(walkStrafeLeftClip, WALK, WrapMode.Loop);
        RegisterClip(idleClip, IDLE, WrapMode.Loop);
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (ghostAnimation[IDLE] != null)
            ghostAnimation.Play(IDLE);
    }

    private void RegisterClip(
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
        if (hasTriggered || player == null)
            return;

        Vector3 playerPosition = player.position;
        Vector3 ghostPosition = ghostRoot.position;

        playerPosition.y = 0f;
        ghostPosition.y = 0f;

        float distance = Vector3.Distance(
            playerPosition,
            ghostPosition
        );

        if (distance <= triggerDistance)
        {
            hasTriggered = true;
            StartCoroutine(MoveLeft());
        }
    }

    private IEnumerator MoveLeft()
    {
        if (ghostAnimation[WALK] == null)
        {
            Debug.LogError("Chua gan Walk Strafe Left!");
            yield break;
        }

        isWalking = true;

        // Luu vi tri va huong ban dau
        Vector3 startPosition = ghostRoot.position;

        // Tinh vi tri ben trai cua ma
        Vector3 leftDirection = -ghostRoot.right;

        leftDirection.y = 0f;
        leftDirection.Normalize();

        Vector3 targetPosition =
            startPosition + leftDirection * walkDistance;

        // Chay animation di ngang trai
        AnimationState walk =
            ghostAnimation[WALK];

        walk.time = 0f;
        walk.speed = animationSpeed;
        walk.wrapMode = WrapMode.Loop;

        ghostAnimation.CrossFade(
            WALK,
            transitionTime
        );

        // Di chuyen muot sang trai
        while (Vector3.Distance(
            ghostRoot.position,
            targetPosition
        ) > 0.05f)
        {
            ghostRoot.position = Vector3.MoveTowards(
                ghostRoot.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Dat chinh xac vi tri ket thuc
        ghostRoot.position = targetPosition;

        isWalking = false;

        // Chuyen sang Idle
        if (ghostAnimation[IDLE] != null)
        {
            ghostAnimation.CrossFade(
                IDLE,
                transitionTime
            );
        }
        else
        {
            ghostAnimation.Stop();
        }

        // Tuy chon an ma
        if (hideAfterWalking)
            ghostRoot.gameObject.SetActive(false);

        Debug.Log("Ma da di sang trai!");
    }
}
