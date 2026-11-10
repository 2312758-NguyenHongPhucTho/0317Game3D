using UnityEngine;

namespace FpsHorrorKit
{
    /// <summary>
    /// Optional first aid pickup: E heals instantly, no inventory needed.
    /// Collider must NOT be a trigger for PlayerInteract raycasts.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class HealingPickup0317 : MonoBehaviour, IInteractable
    {
        [SerializeField] private PlayerHealth0317 playerHealth;
        [SerializeField, Min(1)] private int healAmount = 30;
        [SerializeField] private string interactText = "Nhấn [E] để dùng băng gạc (+30 HP)";
        private bool consumed;

        public void Interact()
        {
            if (consumed) return;
            if (playerHealth == null)
                playerHealth = FindFirstObjectByType<PlayerHealth0317>();
            if (playerHealth == null || !playerHealth.Heal(healAmount)) return;
            consumed = true;
            gameObject.SetActive(false);
            InteractMessageScript.Instance?.ShowMessage("Đã dùng băng gạc. Hồi " + healAmount + " HP.");
        }

        public void Highlight()
        {
            if (!consumed && PlayerInteract.Instance != null)
                PlayerInteract.Instance.ChangeInteractText(interactText);
        }

        public void HoldInteract() { }
        public void UnHighlight() { }
    }
}
