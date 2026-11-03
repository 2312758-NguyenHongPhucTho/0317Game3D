namespace FpsHorrorKit
{
    using System.Collections;
    using UnityEngine;


    public class DoorSystem : MonoBehaviour, IInteractable
    {
        [Header("Highlight UI")]
        [SerializeField] private string interactText = "Nhấn [E] để mở hoặc đóng cửa";
        [SerializeField] private string doorLockedText = "Cửa vẫn khóa. Cần tìm chìa khóa.";
        [SerializeField] private string useKeyText = "Nhấn [E] để dùng chìa khóa";

        [Header("Door Settings")]
        [Tooltip("Kapı kilitli mi?")] public bool isLocked;
        [Tooltip("Kapının anahtarına sahip mi?")] public bool hasKey;
        [Tooltip("Kapının menteşe etrafında dönme hızı")] public float rotationSpeed = 100f;
        public float endRotation;
        public AudioSource doorAudioSource;

        [Header("0317 ritual door gate (optional)")]
        [SerializeField] private RitualDoorSeal0317 ritualDoorSeal;
        [SerializeField] private RitualRoomDoorLock ritualRoomDoorLock;


        private float startRotation = 0;
        private bool isFinished = false;
        private bool isOpen;

        public bool IsOpen => isOpen;
        public bool IsMoving => !isFinished;

        private void Start()
        {
            isFinished = true;
            startRotation = transform.localEulerAngles.y;
        }

        public void Interact()
        {
            if (ritualRoomDoorLock != null && ritualRoomDoorLock.IsLocked) return;
            if (ritualDoorSeal != null && !ritualDoorSeal.IsSealRemoved)
            {
                ritualDoorSeal.NotifyDoorBlocked();
                return;
            }
            if (isLocked)
            {
                if (!isFinished) return;
                if (hasKey)
                {
                    isLocked = false;
                }
                else if (DoorKeyUseController.Instance != null && DoorKeyUseController.Instance.HasKeyFor(this))
                {
                    isLocked = false;
                    DoorKeyUseController.Instance.ConsumeKeyFor(this);
                }
                else
                {
                    return;
                }
            }

            if (!isOpen && isFinished)
            {
                StartCoroutine(OpenDoor(endRotation));
                isOpen = true;
            }
            else if (isOpen && isFinished)
            {
                StartCoroutine(OpenDoor(startRotation));
                isOpen = false;
            }

        }

        public void Highlight()
        {
            if (ritualRoomDoorLock != null && ritualRoomDoorLock.IsLocked) return;
            if (ritualDoorSeal != null && !ritualDoorSeal.IsSealRemoved)
            {
                PlayerInteract.Instance.ChangeInteractText("BÙA PHONG ẤN CHƯA ĐƯỢC GỠ!");
                return;
            }
            if ((hasKey || (DoorKeyUseController.Instance != null && DoorKeyUseController.Instance.HasKeyFor(this))) && isLocked)
            {
                PlayerInteract.Instance.ChangeInteractText(useKeyText);
            }
            else if (isLocked)
            {
                PlayerInteract.Instance.ChangeInteractText(doorLockedText);
            }
            else
            {
                PlayerInteract.Instance.ChangeInteractText(interactText);
            }
        }

        IEnumerator OpenDoor(float targetRotation)
        {
            isFinished = false;
            if (doorAudioSource != null) doorAudioSource.Play();

            while (Mathf.Abs(Mathf.DeltaAngle(transform.localEulerAngles.y, targetRotation)) > 0.1f)
            {
                float step = rotationSpeed * Time.deltaTime;
                float newY = Mathf.MoveTowardsAngle(transform.localEulerAngles.y, targetRotation, step);
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, newY, transform.localEulerAngles.z);
                yield return null;
            }
            // Son rotasyonu kesinleştir
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, targetRotation, transform.localEulerAngles.z);
            isFinished = true;
            Debug.Log("Door opened");
        }
        public void HoldInteract() { }
        public void UnHighlight() { }
    }
}
