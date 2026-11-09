namespace FpsHorrorKit
{
    using UnityEngine;

    public class ITOCameraBattery : MonoBehaviour, IInteractable
    {
        [SerializeField] private Item itemCameraBattery;
        [SerializeField] private string interactText = "Nhấn [E] để nhặt pin máy ảnh";

        public void Interact()
        {
            bool result = Inventory.Instance.AddItem(itemCameraBattery, 1);
            if (result)
            {
                InteractMessageScript.Instance?.ShowMessage("Đã nhặt pin máy ảnh. Nhấn [I] để mở túi đồ và sử dụng.");
                UIInventory.Instance.UpdateUI();
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventory is full!");
            }
        }
        public void UpdateBattery()
        {
            Debug.Log("Camera Battery updated!");
        }
        public void Highlight()
        {
            PlayerInteract.Instance.ChangeInteractText(interactText);
        }

        public void HoldInteract() { }
        public void UnHighlight() { }
    }
}
