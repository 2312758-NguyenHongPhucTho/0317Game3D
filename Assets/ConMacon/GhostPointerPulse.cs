using UnityEngine;

/// <summary>
/// Adds a subtle hovering and pulsing motion to the spectral pointing-hand effect.
/// </summary>
public sealed class GhostPointerPulse : MonoBehaviour
{
    [SerializeField] private float hoverAmplitude = 0.035f;
    [SerializeField] private float hoverFrequency = 2.1f;
    [SerializeField] private float pulseAmplitude = 0.10f;
    [SerializeField] private float pulseFrequency = 3.2f;

    private Vector3 initialPosition;
    private Vector3 initialScale;

    private void Awake()
    {
        initialPosition = transform.localPosition;
        initialScale = transform.localScale;
    }

    private void Update()
    {
        float time = Time.time;
        float hover = Mathf.Sin(time * hoverFrequency) * hoverAmplitude;
        float pulse = 1f + Mathf.Sin(time * pulseFrequency) * pulseAmplitude;

        transform.localPosition = initialPosition + new Vector3(0f, hover, 0f);
        transform.localScale = initialScale * pulse;
    }
}
