namespace FpsHorrorKit
{
    using UnityEngine;

    public class ITOKey : MonoBehaviour, IInteractable
    {
        public DoorSystem compatibleDoor;
        [SerializeField] private string interactText = "Nhấn [E] để nhặt chìa khóa";

        public void Interact()
        {
            compatibleDoor.hasKey = true;
            InteractMessageScript.Instance?.ShowMessage("Đã nhặt chìa khóa.");
            Destroy(gameObject);
        }
        public void Highlight()
        {
            PlayerInteract.Instance.ChangeInteractText(interactText);
        }
        public void HoldInteract() { }
        public void UnHighlight() { }
    }
}
