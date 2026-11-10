namespace FpsHorrorKit
{
    using UnityEngine;
    using UnityEngine.UI;

    public class DamageVignette0317 : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerHealth0317 playerHealth;
        [SerializeField] private Image vignetteImage;
        [SerializeField] private Image hitFlashImage;

        [Header("Timed hit vignette")]
        [SerializeField, Range(0.1f, 10f)] private float hitDurationSeconds = 5f;
        [SerializeField, Range(0.01f, 1f)] private float hitFadeInSeconds = 0.1f;
        [SerializeField, Range(0f, 0.35f)] private float hitVignetteAlpha = 0.32f;

        [Header("Hit Flash")]
        [SerializeField, Range(0f, 0.35f)] private float hitFlashAlpha = 0.35f;
        [SerializeField] private float hitFlashFadeSpeed = 4.5f;

        [Header("Pulse At Low HP")]
        [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.3f;
        [SerializeField, Range(0f, 0.35f)] private float permanentLowHealthAlpha = 0.3f;
        [SerializeField] private bool pulseWhenLow = true;
        [SerializeField] private float pulseSpeed = 3.2f;
        [SerializeField, Range(0f, 0.025f)] private float pulseStrength = 0.025f;

        private float _currentBaseAlpha;
        private float _targetBaseAlpha;
        private float _currentFlashAlpha;
        private float _hitUntilUnscaledTime = -1f;

        private float _lastHealth = -1f;

        private void Start()
        {
            if (playerHealth == null)
                playerHealth = GetComponent<PlayerHealth0317>();

            EnsureUi();
            UpdateVisualImmediate();
        }

        private void Update()
        {
            if (playerHealth == null)
                return;

            UpdateDamageFlashDetection();

            float currentHealth = playerHealth.CurrentHealth;
            float maxHealth = playerHealth.MaxHealth;

            float normalizedHealth = Mathf.Clamp01(currentHealth / maxHealth);
            bool lowHealth = currentHealth > 0 && normalizedHealth <= lowHealthThreshold;
            if (currentHealth <= 0)
            {
                _hitUntilUnscaledTime = -1f;
                _currentFlashAlpha = 0f;
                _targetBaseAlpha = 0f;
            }
            else if (lowHealth)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                _targetBaseAlpha = permanentLowHealthAlpha + (pulseWhenLow ? pulse * pulseStrength : 0f);
            }
            else if (Time.unscaledTime < _hitUntilUnscaledTime - 0.25f)
                _targetBaseAlpha = hitVignetteAlpha;
            else
                _targetBaseAlpha = 0f;

            _targetBaseAlpha = Mathf.Clamp(_targetBaseAlpha, 0f, 0.35f);

            float fadeSeconds = _targetBaseAlpha > _currentBaseAlpha ? hitFadeInSeconds : 0.25f;
            _currentBaseAlpha = Mathf.MoveTowards(_currentBaseAlpha, _targetBaseAlpha, Time.unscaledDeltaTime / fadeSeconds);
            _currentFlashAlpha = Mathf.Lerp(_currentFlashAlpha, 0f, Time.deltaTime * hitFlashFadeSpeed);

            ApplyVisual();
        }

        private void UpdateDamageFlashDetection()
        {
            float hp = playerHealth.CurrentHealth;

            if (_lastHealth < 0f)
            {
                _lastHealth = hp;
                return;
            }

            if (hp < _lastHealth)
            {
                TriggerHitFlash();
            }
            else if (hp > _lastHealth && hp > playerHealth.MaxHealth * lowHealthThreshold)
            {
                _hitUntilUnscaledTime = -1f;
                _currentFlashAlpha = 0f;
            }

            _lastHealth = hp;
        }

        public void TriggerHitFlash()
        {
            _hitUntilUnscaledTime = Time.unscaledTime + hitDurationSeconds;
            _currentFlashAlpha = hitFlashAlpha;
        }

        private void ApplyVisual()
        {
            if (vignetteImage != null)
            {
                Color c = vignetteImage.color;
                c.a = _currentBaseAlpha;
                vignetteImage.color = c;
            }

            if (hitFlashImage != null)
            {
                Color c = hitFlashImage.color;
                c.a = _currentFlashAlpha;
                hitFlashImage.color = c;
            }
        }

        private void UpdateVisualImmediate()
        {
            if (playerHealth == null)
                return;

            _currentBaseAlpha = playerHealth.CurrentHealth > 0 && playerHealth.HealthNormalized <= lowHealthThreshold
                ? permanentLowHealthAlpha
                : 0f;
            _currentFlashAlpha = 0f;
            _hitUntilUnscaledTime = -1f;
            _lastHealth = playerHealth.CurrentHealth;
            ApplyVisual();
        }

        private void EnsureUi()
        {
            if (vignetteImage != null && hitFlashImage != null) return;

            GameObject root = new GameObject("DamageVignetteCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2000;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            Sprite edgeSprite = CreateEdgeSprite();
            vignetteImage = CreateImage("VignetteImage", root.transform, edgeSprite);
            hitFlashImage = CreateImage("HitFlashImage", root.transform, edgeSprite);
        }

        private static Image CreateImage(string imageName, Transform parent, Sprite sprite)
        {
            GameObject go = new GameObject(imageName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = new Color(0.82f, 0.02f, 0.02f, 0f);
            image.raycastTarget = false;
            return image;
        }

        private static Sprite CreateEdgeSprite()
        {
            const int width = 128;
            const int height = 72;
            const float edgeWidth = 0.23f;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "DamageVignetteEdgeTexture";
            texture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < height; y++)
            {
                float v = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / (width - 1);
                    float nearestEdge = Mathf.Min(u, 1f - u, v, 1f - v);
                    float edge = Mathf.Clamp01((edgeWidth - nearestEdge) / edgeWidth);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, edge * edge));
                }
            }
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f));
        }
    }
}
