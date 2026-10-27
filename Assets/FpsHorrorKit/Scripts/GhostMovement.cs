
using UnityEngine;
using System.Collections;

public class GhostMovement : MonoBehaviour
{
    [Header("Di chuyen")]
    public float speed = 2f;
    public bool isAutoMoving = true;

    [Header("Huong di chuyen")]
    public bool moveForward = true;

    [Header("Animation")]
    public string walkAnimation = "Armature|Stalking 3";
    public string idleAnimation = "Armature|Stalking 2";

    [Header("Xuat hien khi nguoi choi den gan")]
    public Transform player;
    public float appearDistance = 5f;
    public float fadeDuration = 2f;

    private Animator animator;
    private bool previousMoving;

    private Renderer[] ghostRenderers;
    private Material[] ghostMaterials;
    private Color[] originalColors;
    private string[] colorProperties;

    private bool hasAppeared = false;
    private bool isAppearing = false;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;

        if (player == null && Camera.main != null)
            player = Camera.main.transform;

        // Lay tat ca renderer cua con ma
        ghostRenderers = GetComponentsInChildren<Renderer>();

        System.Collections.Generic.List<Material> mats =
            new System.Collections.Generic.List<Material>();

        System.Collections.Generic.List<Color> colors =
            new System.Collections.Generic.List<Color>();

        System.Collections.Generic.List<string> props =
            new System.Collections.Generic.List<string>();

        foreach (Renderer r in ghostRenderers)
        {
            foreach (Material mat in r.materials)
            {
                string prop = mat.HasProperty("_BaseColor")
                    ? "_BaseColor"
                    : mat.HasProperty("_Color") ? "_Color" : "";

                if (prop != "")
                {
                    mats.Add(mat);
                    colors.Add(mat.GetColor(prop));
                    props.Add(prop);
                }
            }
        }

        ghostMaterials = mats.ToArray();
        originalColors = colors.ToArray();
        colorProperties = props.ToArray();

        // An con ma luc bat dau
        foreach (Renderer r in ghostRenderers)
            r.enabled = false;

        previousMoving = false;
    }

    void Update()
    {
        // Cho nguoi choi den gan
        if (!hasAppeared)
        {
            if (!isAppearing && player != null)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    player.position
                );

                if (distance <= appearDistance)
                    StartCoroutine(AppearRoutine());
            }

            return;
        }

        MoveGhost();
    }

    IEnumerator AppearRoutine()
    {
        isAppearing = true;

        // Bat dau trong suot
        SetAlpha(0f);

        foreach (Renderer r in ghostRenderers)
            r.enabled = true;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.SmoothStep(0f, 1f, t);

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(1f);

        hasAppeared = true;
        isAppearing = false;

        previousMoving = !isAutoMoving;
        UpdateAnimation();
    }

    void SetAlpha(float alpha)
    {
        for (int i = 0; i < ghostMaterials.Length; i++)
        {
            Color color = originalColors[i];
            color.a = originalColors[i].a * alpha;

            ghostMaterials[i].SetColor(
                colorProperties[i],
                color
            );
        }
    }

    void MoveGhost()
    {
        UpdateAnimation();

        if (!isAutoMoving)
            return;

        Vector3 direction = moveForward
            ? transform.forward
            : -transform.forward;

        direction.y = 0f;
        direction.Normalize();

        transform.position +=
            direction * speed * Time.deltaTime;
    }

    void UpdateAnimation()
    {
        if (animator == null)
            return;

        if (previousMoving == isAutoMoving)
            return;

        previousMoving = isAutoMoving;

        animator.CrossFade(
            isAutoMoving ? walkAnimation : idleAnimation,
            0.2f
        );
    }
}
