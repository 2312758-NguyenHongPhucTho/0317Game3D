using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FpsHorrorKit;

public class NhayMat : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Trigger Settings")]
    public float disappearDistance = 1f; // Khoảng cách đến gần (mét) để kích hoạt nháy mắt
    public bool destroyAfterDisappear = true;

    [Header("Blink on Disappear")]
    public PlayerBlinkEffect playerBlinkEffect;

    private Renderer[] ghostRenderers;
    private Material[] ghostMaterials;
    private Color[] originalColors;
    private string[] colorProperties;

    private bool ghostHidden;
    private bool hasDisappeared = false;

    void Start()
    {
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

        if (playerBlinkEffect == null)
            playerBlinkEffect = FindFirstObjectByType<PlayerBlinkEffect>();
    }

    void Update()
    {
        if (hasDisappeared || player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Khi player bước vào phạm vi khoảng cách -> Kích hoạt chớp mắt và biến mất
        if (distance <= disappearDistance)
        {
            StartCoroutine(DisappearGhost());
        }
    }

    IEnumerator DisappearGhost()
    {
        hasDisappeared = true;

        // Gọi hiệu ứng nháy mắt và ẩn con ma ngay trong lúc màn hình chớp
        if (playerBlinkEffect != null)
        {
            yield return StartCoroutine(playerBlinkEffect.Blink(HideGhostImmediately));
        }
        else
        {
            HideGhostImmediately();
        }

        if (destroyAfterDisappear)
        {
            Destroy(gameObject);
        }
    }

    void SetAlpha(float alpha)
    {
        for (int i = 0; i < ghostMaterials.Length; i++)
        {
            Color color = originalColors[i];
            color.a = originalColors[i].a * alpha;
            ghostMaterials[i].SetColor(colorProperties[i], color);
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