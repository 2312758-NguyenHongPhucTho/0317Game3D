namespace FpsHorrorKit
{
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class LetterUIManager : MonoBehaviour
    {
        public static LetterUIManager Instance { get; private set; }

        [Header("Letter UI")]
        [SerializeField] private GameObject _letterUI;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Button _closeButton;

        [Header("Custom Letter Image")]
        [SerializeField] private Image _letterImage;

        [Header("Typing")]
        [SerializeField] private bool isTyping = false;
        [SerializeField] private float typingDelay = 0.1f;

        private bool isOpen = false;
        private FpsController _fpsController;

        void Awake()
        {
            // Giữ Manager đầu tiên cho các object cũ dùng Instance.
            // Không xóa Manager thứ hai.
            if (Instance == null)
            {
                Instance = this;
            }
        }

        void Start()
        {
            _fpsController = FindAnyObjectByType<FpsController>();

            _closeButton.onClick.AddListener(HideText);

            if (_letterImage != null)
            {
                _letterImage.gameObject.SetActive(false);
            }
        }

        public void ShowText(string text)
        {
            if (isOpen) return;

            if (_letterImage != null)
            {
                _letterImage.gameObject.SetActive(false);
            }

            _text.gameObject.SetActive(true);
            _text.text = "";

            OpenLetter();

            if (isTyping)
            {
                StartCoroutine(Typing(text, typingDelay));
            }
            else
            {
                _text.text = text;
            }
        }

        public void ShowImage(Sprite image)
        {
            if (isOpen) return;

            if (image == null)
            {
                Debug.LogWarning("Letter image is null!");
                return;
            }

            if (_letterImage == null)
            {
                Debug.LogError("Chua gan Letter Image trong Inspector!");
                return;
            }

            _text.text = "";
            _text.gameObject.SetActive(false);

            _letterImage.sprite = image;
            _letterImage.preserveAspect = true;
            _letterImage.gameObject.SetActive(true);

            OpenLetter();
        }

        private void OpenLetter()
        {
            _letterUI.SetActive(true);
            isOpen = true;

            InteractCameraSettings.Instance?.Interacting(.1f);
            InteractCameraSettings.Instance?.ShowCursor();

            PlayerInteract.Instance.sendRaycast = false;

            if (_fpsController != null)
            {
                _fpsController.isInteracting = true;
            }
        }

        public void HideText()
        {
            StopAllCoroutines();

            _letterUI.SetActive(false);

            _text.text = "";
            _text.gameObject.SetActive(true);

            if (_letterImage != null)
            {
                _letterImage.gameObject.SetActive(false);
                _letterImage.sprite = null;
            }

            isOpen = false;

            InteractCameraSettings.Instance?.NotInteracting();
            InteractCameraSettings.Instance?.HideCursor();

            PlayerInteract.Instance.sendRaycast = true;

            if (_fpsController != null)
            {
                _fpsController.isInteracting = false;
            }
        }

        IEnumerator Typing(string newText, float delay)
        {
            foreach (char letter in newText.ToCharArray())
            {
                _text.text += letter;
                yield return new WaitForSeconds(delay);
            }
        }
    }
}