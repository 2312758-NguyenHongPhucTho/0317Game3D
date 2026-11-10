using UnityEngine;

namespace FpsHorrorKit
{
    // Kept in its own file so Unity can serialize this MonoBehaviour on the pickup.
    public class Flashlight0317Pickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactText = "Mình đã thấy nó ở đâu rồi. Nhấn [E] nhặt";

        public void Interact()
        {
            Camera camera = Camera.main;
            if (camera == null)
                return;

            Flashlight0317Controller controller = camera.GetComponent<Flashlight0317Controller>();
            if (controller == null)
                controller = camera.gameObject.AddComponent<Flashlight0317Controller>();

            controller.GiveFlashlight(gameObject);
        }

        public void Highlight()
        {
            PlayerInteract.Instance?.ChangeInteractText(interactText);
        }

        public void HoldInteract() { }
        public void UnHighlight() { }
    }
}
