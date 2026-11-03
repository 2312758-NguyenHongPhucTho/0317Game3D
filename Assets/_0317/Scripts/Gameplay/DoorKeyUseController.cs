using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FpsHorrorKit
{
    public class DoorKeyUseController : MonoBehaviour
    {
        [SerializeField] private float useRange = 2f;

        private sealed class KeyRecord
        {
            public DoorSystem door;
            public DragToOpenSystem dragDoor;
            public GameObject visual;
        }

        public static DoorKeyUseController Instance { get; private set; }

        private readonly List<KeyRecord> keys = new List<KeyRecord>();
        private KeyRecord heldKey;
        private bool hasCollectedKey;
        private FpsController fpsController;
        private PlayerInteract playerInteract;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            fpsController = GetComponentInParent<FpsController>();
            if (fpsController == null)
                fpsController = FindFirstObjectByType<FpsController>();
            playerInteract = FindFirstObjectByType<PlayerInteract>();
        }

        public void GiveKey(DoorSystem door, GameObject keyModel)
        {
            if (door == null || keyModel == null)
                return;

            AddKey(keyModel, door, null);
        }

        public void GiveKey(DragToOpenSystem door, GameObject keyModel)
        {
            if (door == null || keyModel == null)
                return;

            AddKey(keyModel, null, door);
        }

        public bool HasKeyFor(DoorSystem door)
        {
            return FindKey(door) != null;
        }

        public bool ConsumeKeyFor(DoorSystem door)
        {
            KeyRecord key = FindKey(door);
            if (key == null)
                return false;

            Consume(key);
            InteractMessageScript.Instance?.ShowMessage("Đã sử dụng chìa khóa.");
            return true;
        }

        private void AddKey(GameObject keyModel, DoorSystem door, DragToOpenSystem dragDoor)
        {
            KeyRecord key = new KeyRecord { door = door, dragDoor = dragDoor, visual = keyModel };
            keys.Add(key);
            hasCollectedKey = true;

            key.visual.name = "HeldKey_Left";
            key.visual.transform.SetParent(transform, false);
            key.visual.transform.localPosition = new Vector3(-0.30f, -0.23f, 0.52f);
            key.visual.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);

            foreach (Collider collider in key.visual.GetComponentsInChildren<Collider>())
                collider.enabled = false;

            DoorKeyPickup pickup = key.visual.GetComponent<DoorKeyPickup>();
            if (pickup != null)
                Destroy(pickup);

            SetHeldKey(null);
            InteractMessageScript.Instance?.ShowMessage("Đã nhặt chìa khóa. Nhấn [T] để cầm hoặc cất chìa khóa.");
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                if (heldKey != null)
                {
                    SetHeldKey(null);
                    InteractMessageScript.Instance?.ShowMessage("Đã cất chìa khóa.");
                }
                else
                {
                    KeyRecord nextKey = GetLastAvailableKey();
                    if (nextKey == null)
                    {
                        InteractMessageScript.Instance?.ShowMessage(hasCollectedKey ? "Bạn không còn chìa khóa." : "Bạn chưa có chìa khóa.");
                    }
                    else
                    {
                        SetHeldKey(nextKey);
                        InteractMessageScript.Instance?.ShowMessage("Đang cầm chìa khóa.");
                    }
                }
            }

            if (heldKey != null)
                heldKey.visual.SetActive(!ShouldHideKey());

            if (heldKey != null && heldKey.dragDoor != null && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                TryUnlockDragDoor(heldKey);
        }

        private KeyRecord FindKey(DoorSystem door)
        {
            foreach (KeyRecord key in keys)
                if (key.door == door)
                    return key;
            return null;
        }

        private KeyRecord GetLastAvailableKey()
        {
            for (int i = keys.Count - 1; i >= 0; i--)
                if (keys[i].visual != null)
                    return keys[i];
            return null;
        }

        private bool ShouldHideKey()
        {
            return (fpsController != null && (!fpsController.isActiveAndEnabled || fpsController.isInteracting)) ||
                   (playerInteract != null && !playerInteract.isActiveAndEnabled);
        }

        private void SetHeldKey(KeyRecord key)
        {
            if (heldKey != null && heldKey.visual != null)
                heldKey.visual.SetActive(false);

            heldKey = key;
            if (heldKey != null && heldKey.visual != null)
                heldKey.visual.SetActive(!ShouldHideKey());
        }

        private void Consume(KeyRecord key)
        {
            if (heldKey == key)
                heldKey = null;

            keys.Remove(key);
            if (key.visual != null)
                Destroy(key.visual);
        }

        private void TryUnlockDragDoor(KeyRecord key)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit[] hits = Physics.RaycastAll(ray, useRange);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.GetComponent<DragToOpenSystem>() != key.dragDoor)
                    continue;

                key.dragDoor.UnlockAndOpen();
                Consume(key);
                InteractMessageScript.Instance?.ShowMessage("Đã sử dụng chìa khóa.");
                return;
            }
        }
    }
}
