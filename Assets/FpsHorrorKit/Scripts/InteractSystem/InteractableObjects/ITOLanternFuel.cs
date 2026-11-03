using UnityEngine;

namespace FpsHorrorKit
{
    public class ITOLanternFuel : MonoBehaviour, IInteractable
    {
        [SerializeField] private Item itemLanternFuel;
        [SerializeField] private string interactText = "Nhấn [E] để nhặt dầu đèn lồng";

        public void Interact()
        {
            bool result = Inventory.Instance.AddItem(itemLanternFuel, 1);
            if (result)
            {
                InteractMessageScript.Instance?.ShowMessage("Đã nhặt dầu đèn lồng. Nhấn [I] để mở túi đồ và sử dụng.");
                UIInventory.Instance.UpdateUI();
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventory is full!");
            }
        }
        public void Highlight()
        {
            PlayerInteract.Instance.ChangeInteractText(interactText);
        }

        public void HoldInteract() { }
        public void UnHighlight() { }
    }
}
