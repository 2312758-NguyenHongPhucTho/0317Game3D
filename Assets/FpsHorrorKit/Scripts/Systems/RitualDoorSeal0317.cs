using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FpsHorrorKit
{
    [DisallowMultipleComponent]
    public sealed class RitualDoorSeal0317 : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject sealVisual;
        [SerializeField] private Light sealLight;
        [SerializeField] private DamageTrap0317 fireTrap;
        [SerializeField] private GameObject fireVisual;
        [SerializeField] private float fireSeconds = 3f;
        [SerializeField] private float fireCooldownSeconds = 4f;
        [SerializeField] private float holdRemoveSeconds = 2f;
        [SerializeField] private RitualSealSymbol0317[] symbols;

        private bool isSealRemoved;
        private bool allSymbolsSolved;
        private Coroutine removeRoutine;
        private float nextFireTime;
        private Collider interactionCollider;
        private int progress;

        public bool IsSealRemoved { get { return isSealRemoved; } }
        public bool AllSymbolsSolved { get { return allSymbolsSolved; } }

        private void Awake()
        {
            interactionCollider = GetComponent<Collider>();
            if (fireTrap != null) fireTrap.SetTrapActive(false);
            if (fireVisual != null) fireVisual.SetActive(false);
        }

        public void Interact()
        {
            if (isSealRemoved) return;
            if (!allSymbolsSolved)
                InteractMessageScript.Instance?.ShowMessage("Cần phá Tam Ấn Phong Môn.");
            else if (removeRoutine == null)
                removeRoutine = StartCoroutine(HoldToRemove());
        }

        public void Highlight()
        {
            if (isSealRemoved) return;
            PlayerInteract.Instance?.ChangeInteractText(allSymbolsSolved ? "Nhấn giữ [E] để gỡ bùa" : "Phong ấn còn quá mạnh. Cần hóa giải Tam Ấn.");
        }

        public void HoldInteract() { }
        public void UnHighlight() { }

        public void NotifyDoorBlocked()
        {
            if (isSealRemoved || Time.unscaledTime < nextFireTime) return;
            nextFireTime = Time.unscaledTime + fireCooldownSeconds;
            StartCoroutine(FireRoutine());
            InteractMessageScript.Instance?.ShowMessage("BÙA PHONG ẤN CHƯA ĐƯỢC GỠ!");
        }

        public void ActivateSymbol(RitualSealSymbol0317 symbol)
        {
            if (isSealRemoved || symbol == null || symbol.IsSolved) return;
            if (symbol.Order != progress)
            {
                progress = 0;
                if (symbols != null)
                    foreach (RitualSealSymbol0317 item in symbols)
                        if (item != null) item.SetSolved(false);
                InteractMessageScript.Instance?.ShowMessage("Thứ tự phong ấn không chính xác!");
                NotifyDoorBlocked();
                return;
            }
            symbol.SetSolved(true);
            progress++;
            InteractMessageScript.Instance?.ShowMessage("Phong ấn: " + progress + "/3");
            if (progress < 3) return;
            allSymbolsSolved = true;
            InteractMessageScript.Instance?.ShowMessage("Phong ấn đã suy yếu. Hãy trở lại cửa và gỡ lá bùa.");
        }

        private IEnumerator HoldToRemove()
        {
            float held = 0f;
            while (held < holdRemoveSeconds)
            {
                if (Keyboard.current == null || !Keyboard.current.eKey.isPressed || PlayerInteract.Instance == null || !PlayerInteract.Instance.isActiveAndEnabled || Camera.main == null || Vector3.Distance(Camera.main.transform.position, transform.position) > 2.5f)
                {
                    InteractMessageScript.Instance?.ShowMessage("Đã hủy gỡ bùa.");
                    removeRoutine = null;
                    yield break;
                }
                held += Time.deltaTime;
                PlayerInteract.Instance?.ChangeInteractText("Đang gỡ bùa... " + Mathf.RoundToInt(held / holdRemoveSeconds * 100f) + "%");
                yield return null;
            }
            isSealRemoved = true;
            if (interactionCollider != null) interactionCollider.enabled = false;
            if (sealVisual != null) sealVisual.SetActive(false);
            if (sealLight != null) sealLight.enabled = false;
            if (fireTrap != null) fireTrap.SetTrapActive(false);
            if (fireVisual != null) fireVisual.SetActive(false);
            InteractMessageScript.Instance?.ShowMessage("Phong ấn đã được hóa giải");
            removeRoutine = null;
        }

        private System.Collections.IEnumerator FireRoutine()
        {
            if (fireVisual != null) fireVisual.SetActive(true);
            if (fireTrap != null) fireTrap.SetTrapActive(true);
            yield return new WaitForSeconds(fireSeconds);
            if (!isSealRemoved)
            {
                if (fireTrap != null) fireTrap.SetTrapActive(false);
                if (fireVisual != null) fireVisual.SetActive(false);
            }
        }
    }
}
