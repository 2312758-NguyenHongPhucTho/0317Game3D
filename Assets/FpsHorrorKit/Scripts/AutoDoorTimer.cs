using UnityEngine;
using System.Collections;

public class AutoDoorTimer : MonoBehaviour
{
    [Header("Cấu hình Cửa")]
    [SerializeField] private Transform doorTransform; // Object 'Door'
    [SerializeField] private float openAngle = 90f;   // Góc mở cửa
    [SerializeField] private float speed = 3f;        // Tốc độ xoay

    [Header("Thời gian chờ tự mở")]
    [SerializeField] private float delayTime = 15f;   // Thời gian chờ (giây)

    [Header("Tương tác phím E & Tự đóng")]
    [SerializeField] private float interactDistance = 3.5f; // Khoảng cách tối đa để ấn phím E
    [SerializeField] private string playerTag = "Player";  // Tag của người chơi

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;

        // Tự động tìm object con "Door" nếu chưa gán
        if (doorTransform == null)
        {
            Transform childDoor = transform.Find("Door");
            doorTransform = (childDoor != null) ? childDoor : transform;
        }

        // Lưu góc đóng ban đầu và tính góc mở
        closedRotation = doorTransform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

        // Bắt đầu đếm ngược 15 giây để tự động mở cửa
        StartCoroutine(OpenDoorAfterDelay());
    }

    private IEnumerator OpenDoorAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        isOpen = true; // Tự động chuyển trạng thái thành mở
    }

    private void Update()
    {
        // Nhấn phím E để tương tác đóng/mở cửa
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryToggleDoor();
        }

        // Xoay cửa mượt mà về vị trí mục tiêu (Đóng hoặc Mở)
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        doorTransform.localRotation = Quaternion.Slerp(doorTransform.localRotation, targetRotation, Time.deltaTime * speed);
    }

    // Kiểm tra nếu Player nhìn vào cửa và bấm phím E
    private void TryToggleDoor()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.transform == transform || hit.transform == doorTransform || hit.transform.IsChildOf(transform))
            {
                isOpen = !isOpen; // Đảo ngược trạng thái cửa
            }
        }
    }

    // Tự động đóng cửa khi Player rời khỏi vùng Trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isOpen = false; // Tự động đóng cửa lại
        }
    }
}