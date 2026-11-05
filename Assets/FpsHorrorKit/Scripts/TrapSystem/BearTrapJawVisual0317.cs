using System.Collections;
using UnityEngine;

namespace FpsHorrorKit
{
    /// <summary>
    /// Optional simple 2-jaw snap animation for a placeholder/model with separate jaw pivots.
    /// Wire SnapShut() and OpenJaws() to TrapEscapeZone0317 UnityEvents.
    /// The starting local rotations are treated as OPEN; adjust close offsets in Inspector.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BearTrapJawVisual0317 : MonoBehaviour
    {
        [SerializeField] private Transform leftJawPivot;
        [SerializeField] private Transform rightJawPivot;
        [SerializeField] private Vector3 leftCloseEulerOffset = new Vector3(0f, 0f, 65f);
        [SerializeField] private Vector3 rightCloseEulerOffset = new Vector3(0f, 0f, -65f);
        [SerializeField, Min(0.01f)] private float snapCloseSeconds = 0.16f;
        [SerializeField, Min(0.01f)] private float reopenSeconds = 0.5f;

        private Quaternion leftOpen;
        private Quaternion rightOpen;
        private Coroutine transition;
        private bool cached;

        private void Awake()
        {
            if (leftJawPivot != null) leftOpen = leftJawPivot.localRotation;
            if (rightJawPivot != null) rightOpen = rightJawPivot.localRotation;
            cached = true;
        }

        public void SnapShut()
        {
            if (!cached) Awake();
            Animate(true, snapCloseSeconds);
        }

        public void OpenJaws()
        {
            if (!cached) Awake();
            Animate(false, reopenSeconds);
        }

        private void Animate(bool close, float seconds)
        {
            if (transition != null) StopCoroutine(transition);
            transition = StartCoroutine(MoveJaws(close, seconds));
        }

        private IEnumerator MoveJaws(bool close, float seconds)
        {
            Quaternion aFrom = leftJawPivot != null ? leftJawPivot.localRotation : Quaternion.identity;
            Quaternion bFrom = rightJawPivot != null ? rightJawPivot.localRotation : Quaternion.identity;
            Quaternion aTo = close ? leftOpen * Quaternion.Euler(leftCloseEulerOffset) : leftOpen;
            Quaternion bTo = close ? rightOpen * Quaternion.Euler(rightCloseEulerOffset) : rightOpen;
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, seconds));
                // Faster snap for close; gentler for reopen.
                t = close ? 1f - Mathf.Pow(1f - t, 3f) : t * t * (3f - 2f * t);
                if (leftJawPivot != null) leftJawPivot.localRotation = Quaternion.Slerp(aFrom, aTo, t);
                if (rightJawPivot != null) rightJawPivot.localRotation = Quaternion.Slerp(bFrom, bTo, t);
                yield return null;
            }
            if (leftJawPivot != null) leftJawPivot.localRotation = aTo;
            if (rightJawPivot != null) rightJawPivot.localRotation = bTo;
            transition = null;
        }
    }
}
