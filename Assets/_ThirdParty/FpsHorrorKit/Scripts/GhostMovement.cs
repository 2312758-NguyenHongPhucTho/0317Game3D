using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FpsHorrorKit;

public class GhostMovement : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Appear Settings")]
    public float appearDistance = 5f;
    public float fadeInDuration = 0.5f;

    [Header("Movement")]
    public float speed = 1.5f;
    public bool moveForward = true;

    [Header("Animation")]
    public string walkAnimation = "Armature|Stalking 3";

    [Header("Disappear Settings")]
    public float disappearDistance = 2f;
    public float rotationDuration = 1f;
    public float fadeOutDuration = 0.5f;
    public bool destroyAfterDisappear = false;

    [Header("Blink on Disappear")]
    public PlayerBlinkEffect playerBlinkEffect;
    public bool blinkOnDisappear = true;

    private Animator animator;
    private Renderer[] ghostRenderers;

    private Material[] ghostMaterials;
    private Color[] originalColors;
    private string[] colorProperties;

    private bool ghostHidden;

    private bool hasAppeared = false;
    private bool isAppearing = false;
    private bool isDisappearing = false;
    private bool hasDisappeared = false;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        if (player == null && Camera.main != null)
        {
            player = Camera.main.transform;
        }

        ghostRenderers = GetComponentsInChildren<Renderer>();

        List<Material> materials = new List<Material>();
        List<Color> colors = new List<Color>();
        List<string> properties = new List<string>();

        foreach (Renderer renderer in ghostRenderers)
        {
            foreach (Material material in renderer.materials)
            {
                string property = "";

                if (material.HasProperty("_BaseColor"))
                    property = "_BaseColor";
                else if (material.HasProperty("_Color"))
                    property = "_Color";

                if (property != "")
                {
                    materials.Add(material);
                    colors.Add(material.GetColor(property));
                    properties.Add(property);
                }
            }
        }

        ghostMaterials = materials.ToArray();
        originalColors = colors.ToArray();
        colorProperties = properties.ToArray();

        foreach (Renderer renderer in ghostRenderers)
        {
            renderer.enabled = false;
        }

        if (playerBlinkEffect == null)
            playerBlinkEffect = FindFirstObjectByType<PlayerBlinkEffect>();

        if (blinkOnDisappear && playerBlinkEffect == null)
        {
            Debug.LogWarning(
                "GhostMovement: PlayerBlinkEffect was not found. " +
                "The ghost will disappear without the blink effect.",
                this
            );
        }
    }

    void Update()
    {
        if (hasDisappeared || player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Player den gan -> Ghost xuat hien
        if (!hasAppeared && !isAppearing)
        {
            if (distance <= appearDistance)
            {
                StartCoroutine(AppearGhost());
            }

            return;
        }

        if (isAppearing || isDisappearing)
            return;

        // Player den qua gan -> Ghost xoay va bien mat
        if (distance <= disappearDistance)
        {
            StartCoroutine(DisappearGhost());
            return;
        }

        if (hasAppeared)
        {
            MoveGhost();
        }
    }

    IEnumerator AppearGhost()
    {
        isAppearing = true;

        SetAlpha(0f);

        foreach (Renderer renderer in ghostRenderers)
        {
            renderer.enabled = true;
        }

        if (animator != null)
        {
            animator.CrossFade(
                walkAnimation,
                0.2f
            );
        }

        yield return StartCoroutine(
            FadeGhost(0f, 1f, fadeInDuration)
        );

        hasAppeared = true;
        isAppearing = false;
    }

    void MoveGhost()
    {
        Vector3 direction = moveForward
            ? transform.forward
            : -transform.forward;

        direction.y = 0f;
        direction.Normalize();

        transform.position +=
            direction * speed * Time.deltaTime;
    }

    IEnumerator DisappearGhost()
    {
        isDisappearing = true;

        // Ghost dung di chuyen va xoay 360 do
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, rotationDuration);

        Quaternion startRotation = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            float angle = 360f * t;

            transform.rotation =
                startRotation * Quaternion.Euler(0f, angle, 0f);

            yield return null;
        }

        transform.rotation = startRotation;

        if (blinkOnDisappear && playerBlinkEffect != null)
        {
            yield return StartCoroutine(
                playerBlinkEffect.Blink(HideGhostImmediately)
            );
        }
        else
        {
            yield return StartCoroutine(
                FadeGhost(1f, 0f, fadeOutDuration)
            );
            HideGhostImmediately();
        }

        hasDisappeared = true;
        isDisappearing = false;

        if (destroyAfterDisappear)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator FadeGhost(
        float startAlpha,
        float endAlpha,
        float duration
    )
    {
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            float alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                t
            );

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(endAlpha);
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

    private void HideGhostImmediately()
    {
        if (ghostHidden)
            return;

        ghostHidden = true;
        SetAlpha(0f);

        foreach (Renderer renderer in ghostRenderers)
        {
            renderer.enabled = false;
        }

        foreach (Collider ghostCollider in GetComponentsInChildren<Collider>())
        {
            ghostCollider.enabled = false;
        }
    }
}
