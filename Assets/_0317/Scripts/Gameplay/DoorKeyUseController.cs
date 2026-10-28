using UnityEngine;
using UnityEngine.InputSystem;

namespace FpsHorrorKit
{
    public class DoorKeyUseController : MonoBehaviour
    {
        [SerializeField] private float useRange = 2f;

        private DoorSystem targetDoor;
        private DragToOpenSystem targetDragDoor;
        private GameObject heldKey;
        private bool isHoldingKey;

        public void GiveKey(DoorSystem door, GameObject keyModel)
        {
            if (door == null || keyModel == null)
                return;

            targetDoor = door;
            targetDragDoor = null;
            SetupHeldKey(keyModel);
        }

        public void GiveKey(DragToOpenSystem door, GameObject keyModel)
        {
            if (door == null || keyModel == null)
                return;

            targetDoor = null;
            targetDragDoor = door;
            SetupHeldKey(keyModel);
        }

        private void SetupHeldKey(GameObject keyModel)
        {
            heldKey = keyModel;
            heldKey.name = "Held_Single_Door_2_Key";
            heldKey.transform.SetParent(transform, false);
            heldKey.transform.localPosition = new Vector3(0.28f, -0.26f, 0.55f);
            heldKey.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);

            foreach (Collider collider in heldKey.GetComponentsInChildren<Collider>())
                collider.enabled = false;

            DoorKeyPickup pickup = heldKey.GetComponent<DoorKeyPickup>();
            if (pickup != null)
                Destroy(pickup);

            SetHolding(false);
            InteractMessageScript.Instance?.ShowMessage($"Door key acquired. Press T to hold it, then click {TargetDoorName}.");
        }

        private void Update()
        {
            if ((targetDoor == null && targetDragDoor == null) || heldKey == null)
                return;

            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                SetHolding(!isHoldingKey);
                InteractMessageScript.Instance?.ShowMessage(isHoldingKey
                    ? $"Key equipped. Click {TargetDoorName} to unlock it."
                    : "Key put away.");
            }

            if (isHoldingKey && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                TryUnlockDoor();
        }

        private void SetHolding(bool holding)
        {
            isHoldingKey = holding;
            if (heldKey != null)
                heldKey.SetActive(holding);
        }

        private void TryUnlockDoor()
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit[] hits = Physics.RaycastAll(ray, useRange);
            if (hits.Length == 0)
                return;

            if (targetDoor != null)
            {
                bool hitTargetDoor = false;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.GetComponent<DoorSystem>() == targetDoor)
                    {
                        hitTargetDoor = true;
                        break;
                    }
                }
                if (!hitTargetDoor)
                    return;

                targetDoor.hasKey = true;
                targetDoor.isLocked = false;
                targetDoor.Interact();
            }
            else
            {
                bool hitTargetDoor = false;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.GetComponent<DragToOpenSystem>() == targetDragDoor)
                    {
                        hitTargetDoor = true;
                        break;
                    }
                }
                if (!hitTargetDoor)
                    return;

                targetDragDoor.UnlockAndOpen();
            }
            SetHolding(false);
            InteractMessageScript.Instance?.ShowMessage($"{TargetDoorName} unlocked.");
        }

        private string TargetDoorName
        {
            get
            {
                Transform doorTransform = targetDoor != null ? targetDoor.transform : targetDragDoor != null ? targetDragDoor.transform : null;
                return doorTransform != null && doorTransform.parent != null ? doorTransform.parent.name : "the door";
            }
        }
    }
}
