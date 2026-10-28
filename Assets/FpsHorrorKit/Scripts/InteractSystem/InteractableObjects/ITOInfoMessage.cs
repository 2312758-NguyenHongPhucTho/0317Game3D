namespace FpsHorrorKit
{
    using UnityEngine;

    public class ITOInfoMessage : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactText = "Read Message [E]";

        [SerializeField]
        [TextArea(3, 10)]
        private string _message;

        [Header("Letter Image")]
        [SerializeField] private Sprite letterImage;

        [Header("Target Letter UI")]
        [SerializeField] private LetterUIManager letterUIManager;

        public void Interact()
        {
            // Dùng Manager được gắn trực tiếp trên từng tờ giấy.
            // Nếu chưa gắn thì dùng Manager mặc định để không lỗi giấy cũ.
            LetterUIManager targetManager = letterUIManager != null
                ? letterUIManager
                : LetterUIManager.Instance;

            if (targetManager == null)
            {
                Debug.LogWarning(
                    "Chua gan Letter UI Manager cho: " + gameObject.name
                );
                return;
            }

            if (letterImage != null)
            {
                targetManager.ShowImage(letterImage);
            }
            else
            {
                targetManager.ShowText(_message);
            }
        }

        public void Highlight()
        {
            PlayerInteract.Instance?.ChangeInteractText(interactText);
        }

        public void UnHighlight()
        {
        }

        public void HoldInteract()
        {
        }
    }
}