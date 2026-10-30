using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FpsHorrorKit
{
    [DisallowMultipleComponent]
    public class PlayerBlinkEffect : MonoBehaviour
    {
        [Header("Timing")]
        [Min(0f)] public float closeDuration = 0.18f;
        [Min(0f)] public float closedDuration = 0.25f;
        [Min(0f)] public float openDuration = 0.3f;
        public bool blinkOnStart = false;
        public bool useUnscaledTime = true;

        private Canvas _canvas;
        private RectTransform _topEyelid;
        private RectTransform _bottomEyelid;
        private float _blinkProgress;
        private bool _isBlinking;

        public bool IsBlinking => _isBlinking;

        private void Awake()
        {
            CreateBlinkUI();
            SetBlinkProgress(0f);
        }

        private void Start()
        {
            if (blinkOnStart)
                StartCoroutine(Blink());
        }

        private void Update()
        {
            // Keep the eyelids aligned when the game resolution changes.
            if (!_isBlinking)
                SetBlinkProgress(_blinkProgress);
        }

        public IEnumerator Blink(Action onFullyClosed = null)
        {
            if (_isBlinking)
                yield break;

            CreateBlinkUI();
            _isBlinking = true;

            yield return AnimateBlink(0f, 1f, closeDuration);
            onFullyClosed?.Invoke();
            yield return WaitForBlinkDuration(closedDuration);
            yield return AnimateBlink(1f, 0f, openDuration);

            _isBlinking = false;
        }

        private void CreateBlinkUI()
        {
            if (_canvas != null)
                return;

            GameObject canvasObject = new GameObject(
                "Player Blink Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(CanvasGroup)
            );
            canvasObject.transform.SetParent(transform, false);

            _canvas = canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 32767;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CanvasGroup canvasGroup = canvasObject.GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            _topEyelid = CreateEyelid("Upper Eyelid", true);
            _bottomEyelid = CreateEyelid("Lower Eyelid", false);
        }

        private RectTransform CreateEyelid(string eyelidName, bool isTop)
        {
            GameObject eyelidObject = new GameObject(
                eyelidName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            eyelidObject.transform.SetParent(_canvas.transform, false);

            Image image = eyelidObject.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;

            RectTransform rect = image.rectTransform;
            float verticalAnchor = isTop ? 1f : 0f;
            rect.anchorMin = new Vector2(0f, verticalAnchor);
            rect.anchorMax = new Vector2(1f, verticalAnchor);
            rect.pivot = isTop ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
            return rect;
        }

        private IEnumerator AnimateBlink(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                SetBlinkProgress(to);
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += BlinkDeltaTime;
                float progress = Mathf.SmoothStep(
                    from,
                    to,
                    Mathf.Clamp01(elapsed / duration)
                );
                SetBlinkProgress(progress);
                yield return null;
            }

            SetBlinkProgress(to);
        }

        private IEnumerator WaitForBlinkDuration(float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += BlinkDeltaTime;
                yield return null;
            }
        }

        private float BlinkDeltaTime =>
            useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        private void SetBlinkProgress(float progress)
        {
            _blinkProgress = Mathf.Clamp01(progress);

            if (_canvas == null || _topEyelid == null || _bottomEyelid == null)
                return;

            Canvas.ForceUpdateCanvases();
            float eyelidHeight = (_canvas.transform as RectTransform).rect.height * 0.5f + 2f;

            _topEyelid.sizeDelta = new Vector2(0f, eyelidHeight);
            _bottomEyelid.sizeDelta = new Vector2(0f, eyelidHeight);

            _topEyelid.anchoredPosition = new Vector2(
                0f,
                Mathf.Lerp(eyelidHeight, 0f, _blinkProgress)
            );
            _bottomEyelid.anchoredPosition = new Vector2(
                0f,
                Mathf.Lerp(-eyelidHeight, 0f, _blinkProgress)
            );
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _isBlinking = false;
            SetBlinkProgress(0f);
        }
    }
}
