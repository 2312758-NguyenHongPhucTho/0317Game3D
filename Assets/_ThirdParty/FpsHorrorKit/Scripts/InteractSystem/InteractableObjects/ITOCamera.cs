namespace FpsHorrorKit
{
    using UnityEngine;

    public class ITOCamera : MonoBehaviour, IInteractable
    {
        public Item photoCamera;
        [SerializeField] private string interactText = "Nhấn [E] để nhặt máy ảnh";

        public void Interact()
        {
            InteractMessageScript.Instance?.ShowMessage("Đã nhặt máy ảnh. Nhấn [2] rồi [T] để sử dụng.");
            photoCamera.hasItem = true;
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
