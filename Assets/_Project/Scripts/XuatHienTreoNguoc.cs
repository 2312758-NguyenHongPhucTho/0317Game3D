using UnityEngine;
using System.Collections;

public class GhostDoorActivity : MonoBehaviour
{
    [Header("DOOR")]
    public Transform doorMovingPart;

    [Header("GHOST")]
    public GameObject ghostObject;

    [Header("SETTINGS")]
    // Đã thay đổi thành 3 giây theo yêu cầu
    public float appearDuration = 3f;

    [Tooltip("Do nhay phat hien xoay cua")]
    public float rotationSensitivity = 0.02f;

    [Tooltip("Do nhay phat hien cua di chuyen")]
    public float positionSensitivity = 0.0005f;

    [Header("TRIGGER")]
    [Tooltip("Bat: chi hien ma lan dau. Tat: hien moi khi cua bat dau di chuyen.")]
    public bool triggerOnlyOnce = true;

    private Vector3 previousPosition;
    private Quaternion previousRotation;

    private bool wasMoving;
    private bool hasTriggered;

    private Coroutine ghostCoroutine;

    private void Awake()
    {
        if (doorMovingPart == null)
        {
            Debug.LogError("Chua gan Door Moving Part!", this);
            enabled = false;
            return;
        }

        previousPosition = doorMovingPart.localPosition;
        previousRotation = doorMovingPart.localRotation;

        if (ghostObject != null)
        {
            ghostObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Chua gan Ghost Object!", this);
        }
    }

    private void LateUpdate()
    {
        if (doorMovingPart == null || ghostObject == null)
            return;

        Vector3 currentPosition = doorMovingPart.localPosition;
        Quaternion currentRotation = doorMovingPart.localRotation;

        // Kiem tra cua co thay doi vi tri
        bool positionChanged = Vector3.Distance(previousPosition, currentPosition) >= positionSensitivity;

        // Kiem tra cua co thay doi goc xoay
        bool rotationChanged = Quaternion.Angle(previousRotation, currentRotation) >= rotationSensitivity;

        bool isMoving = positionChanged || rotationChanged;

        // Chi kich hoat tai thoi diem cua bat dau di chuyen
        if (isMoving && !wasMoving)
        {
            OnDoorActivity();
        }

        wasMoving = isMoving;

        // Cap nhat vi tri va goc cua
        previousPosition = currentPosition;
        previousRotation = currentRotation;
    }

    // Co the goi ham nay tu script mo cua co san
    public void OnDoorActivity()
    {
        if (ghostObject == null)
            return;

        if (triggerOnlyOnce && hasTriggered)
            return;

        hasTriggered = true;

        // Neu ma dang hien thi thi reset thoi gian
        if (ghostCoroutine != null)
        {
            StopCoroutine(ghostCoroutine);
        }

        ghostCoroutine = StartCoroutine(ShowGhost());
    }

    private IEnumerator ShowGhost()
    {
        // Hien ma ngay lap tuc
        ghostObject.SetActive(true);

        Debug.Log("Door activity detected - Ghost appeared!");

        // Đợi trong khoảng thời gian appearDuration (3 giây)
        yield return new WaitForSeconds(appearDuration);

        // An ma sau 3 giay
        ghostObject.SetActive(false);

        ghostCoroutine = null;

        Debug.Log("Ghost disappeared!");
    }
}