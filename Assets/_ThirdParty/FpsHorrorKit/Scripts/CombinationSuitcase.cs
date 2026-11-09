using System.Collections;
using UnityEngine;

namespace FpsHorrorKit
{
    [DisallowMultipleComponent]
    public class CombinationSuitcase : MonoBehaviour, IInteractable
    {
        [Header("Combination")]
        [SerializeField] private string combination = "0317";

        [Header("Opening Animation")]
        [SerializeField] private Transform lidPivot;
        [SerializeField] private Transform clasp;
        [SerializeField, Min(0.05f)] private float openDuration = 0.85f;
        [SerializeField] private float openAngle = 110f;

        [Header("Feedback")]
        [SerializeField] private Renderer statusLight;
        [SerializeField] private Color lockedColor = new Color(0.6f, 0.04f, 0.015f, 1f);
        [SerializeField] private Color unlockedColor = new Color(0.08f, 0.8f, 0.28f, 1f);

        private Quaternion closedLidRotation;
        private bool isOpened;
        private SuitcaseCombinationUI combinationUi;

        public bool IsOpened => isOpened;

        private void Awake()
        {
            if (lidPivot != null)
            {
                closedLidRotation = lidPivot.localRotation;
            }

            SetContentInteraction(false);
            SetStatusLight(lockedColor);
        }

        public void Interact()
        {
            if (!isOpened)
            {
                SuitcaseCombinationUI ui = GetCombinationUi();
                if (!ui.IsOpen)
                {
                    ui.Open(this);
                }
            }
        }

        private void OnMouseDown()
        {
            Interact();
        }

        public void HoldInteract() { }

        public void Highlight()
        {
            if (!isOpened)
            {
                PlayerInteract.Instance?.ChangeInteractText("Nhấn [E] để nhập mã vali");
            }
        }

        public void UnHighlight() { }

        public bool TryUnlock(string enteredCode)
        {
            if (isOpened || enteredCode != combination)
            {
                return false;
            }

            StartCoroutine(OpenCase());
            return true;
        }

        private IEnumerator OpenCase()
        {
            isOpened = true;
            SetContentInteraction(true);
            SetStatusLight(unlockedColor);

            if (clasp != null)
            {
                clasp.gameObject.SetActive(false);
            }

            if (lidPivot == null)
            {
                SetContentInteraction(true);
                yield break;
            }

            Quaternion targetRotation = closedLidRotation * Quaternion.Euler(openAngle, 0f, 0f);
            float elapsed = 0f;
            while (elapsed < openDuration)
            {
                elapsed += Time.deltaTime;
                lidPivot.localRotation = Quaternion.Slerp(
                    closedLidRotation, targetRotation, Mathf.SmoothStep(0f, 1f, elapsed / openDuration));
                yield return null;
            }

            lidPivot.localRotation = targetRotation;
            SetContentInteraction(true);
        }

        private void SetContentInteraction(bool enabled)
        {
            SuitcasePhotoViewer[] photos = GetComponentsInChildren<SuitcasePhotoViewer>(true);
            foreach (SuitcasePhotoViewer photo in photos)
            {
                Collider photoCollider = photo.GetComponent<Collider>();
                if (photoCollider != null)
                {
                    photoCollider.enabled = enabled;
                }
            }
        }

        private SuitcaseCombinationUI GetCombinationUi()
        {
            if (combinationUi != null)
            {
                return combinationUi;
            }

            combinationUi = FindAnyObjectByType<SuitcaseCombinationUI>();
            if (combinationUi == null)
            {
                GameObject host = Camera.main != null ? Camera.main.gameObject : gameObject;
                combinationUi = host.GetComponent<SuitcaseCombinationUI>();
                if (combinationUi == null)
                {
                    combinationUi = host.AddComponent<SuitcaseCombinationUI>();
                }
            }

            return combinationUi;
        }

        private void SetStatusLight(Color color)
        {
            if (statusLight == null)
            {
                return;
            }

            Material material = statusLight.material;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.color = color;
            }
        }
    }
}
