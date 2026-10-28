using UnityEngine;

namespace FpsHorrorKit
{
    public class DoorKeyPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private DoorSystem targetDoor;
        [SerializeField] private DragToOpenSystem targetDragDoor;
        [SerializeField] private string interactText = "Pick up door key [E]";

        public void Configure(DoorSystem door)
        {
            targetDoor = door;
            targetDragDoor = null;
        }

        public void Configure(DragToOpenSystem door)
        {
            targetDoor = null;
            targetDragDoor = door;
        }

        public void Interact()
        {
            if ((targetDoor == null && targetDragDoor == null) || Camera.main == null)
                return;

            DoorKeyUseController keyController = Camera.main.GetComponent<DoorKeyUseController>();
            if (keyController == null)
                keyController = Camera.main.gameObject.AddComponent<DoorKeyUseController>();

            if (targetDoor != null)
                keyController.GiveKey(targetDoor, gameObject);
            else
                keyController.GiveKey(targetDragDoor, gameObject);
        }

        public void Highlight()
        {
            PlayerInteract.Instance?.ChangeInteractText(interactText);
        }

        public void HoldInteract() { }
        public void UnHighlight() { }
    }
}
