using UnityEngine;

namespace FpsHorrorKit
{
    public class Flashlight0317Controller : MonoBehaviour
    {
        [Header("Controls")]
        [SerializeField] private KeyCode toggleKey = KeyCode.R;
        [SerializeField] private bool hasFlashlight;
        [SerializeField] private bool isEquipped;

        [Header("Flashlight Position")]
        [SerializeField]
        private Vector3 holdPosition =
            new Vector3(0.30f, -0.28f, 0.58f);

        [SerializeField]
        private Vector3 holdRotation =
            new Vector3(5f, 0f, 0f);

        [Header("Main Light - Chieu Xa")]
        [SerializeField] private float mainIntensity = 900f;
        [SerializeField] private float mainRange = 18f;
        [SerializeField] private float mainSpotAngle = 55f;
        [SerializeField] private float mainInnerAngle = 35f;

        [Header("Near Light - Sang Rong Gan")]
        [SerializeField] private float nearIntensity = 350f;
        [SerializeField] private float nearRange = 6f;
        [SerializeField] private float nearSpotAngle = 95f;

        [Header("Hand Sway - Do Tre Khi Xoay Chuot")]
        [SerializeField] private float swayAmount = 2f;
        [SerializeField] private float swayPositionAmount = 0.012f;
        [SerializeField] private float swaySmooth = 10f;

        [Header("Walking Bob - Rung Theo Buoc Chan")]
        [SerializeField] private float bobSpeed = 9f;
        [SerializeField] private float bobAmount = 0.012f;

        [Header("References")]
        [SerializeField] private Transform playerRoot;

        private GameObject heldFlashlight;
        private Light beamLight;
        private Light nearLight;

        private Vector3 currentLocalPosition;
        private Quaternion currentLocalRotation;

        private float bobTimer;
        private Vector3 lastPlayerPosition;

        private void Start()
        {
            if (playerRoot == null)
                playerRoot = transform.root;

            if (playerRoot != null)
                lastPlayerPosition = playerRoot.position;
        }

        private void Update()
        {
            if (hasFlashlight && Input.GetKeyDown(toggleKey))
                SetEquipped(!isEquipped);
        }

        private void LateUpdate()
        {
            if (heldFlashlight == null || !isEquipped)
                return;

            UpdateHandMovement();
        }

        public void GiveFlashlight(GameObject flashlightModel)
        {
            if (flashlightModel == null)
                return;

            if (hasFlashlight)
                return;

            hasFlashlight = true;

            AttachFlashlightModel(flashlightModel);
            SetEquipped(true);

            InteractMessageScript.Instance?.ShowMessage(
                "Nhấn [R] để cất hoặc lấy đèn pin"
            );
        }

        private void SetEquipped(bool equipped)
        {
            isEquipped = equipped;

            if (heldFlashlight != null)
                heldFlashlight.SetActive(equipped);
        }

        private void AttachFlashlightModel(GameObject flashlightModel)
        {
            heldFlashlight = flashlightModel;

            heldFlashlight.name = "HeldFlashlight_0317";

            heldFlashlight.transform.SetParent(transform, false);

            heldFlashlight.transform.localPosition = holdPosition;

            heldFlashlight.transform.localRotation =
                Quaternion.Euler(holdRotation);

            currentLocalPosition = holdPosition;
            currentLocalRotation = Quaternion.Euler(holdRotation);

            // Tat collider cua den khi dang cam
            foreach (Collider collider in
                heldFlashlight.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            // Xoa script nhat den sau khi da nhat
            Flashlight0317Pickup pickup =
                heldFlashlight.GetComponent<Flashlight0317Pickup>();

            if (pickup != null)
                Destroy(pickup);

            // Tranh anh huong vat ly khi cam
            foreach (Rigidbody rb in
                heldFlashlight.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            CreateMainLight();
            CreateNearLight();
        }

        private void CreateMainLight()
        {
            GameObject beam = new GameObject("FlashlightBeam_Main");

            beam.transform.SetParent(
                heldFlashlight.transform,
                false
            );

            beam.transform.localPosition =
                new Vector3(0f, 0f, 0.46f);

            beam.transform.localRotation = Quaternion.identity;

            beamLight = beam.AddComponent<Light>();

            beamLight.type = LightType.Spot;

            beamLight.color = new Color(
                1f,
                0.88f,
                0.66f
            );

            beamLight.intensity = mainIntensity;
            beamLight.range = mainRange;
            beamLight.spotAngle = mainSpotAngle;
            beamLight.innerSpotAngle =
                Mathf.Min(mainInnerAngle, mainSpotAngle);

            beamLight.shadows = LightShadows.Soft;
        }

        private void CreateNearLight()
        {
            GameObject nearBeam =
                new GameObject("FlashlightBeam_Near");

            nearBeam.transform.SetParent(
                heldFlashlight.transform,
                false
            );

            nearBeam.transform.localPosition =
                new Vector3(0f, 0f, 0.46f);

            nearBeam.transform.localRotation = Quaternion.identity;

            nearLight = nearBeam.AddComponent<Light>();

            nearLight.type = LightType.Spot;

            nearLight.color = new Color(
                1f,
                0.85f,
                0.65f
            );

            nearLight.intensity = nearIntensity;
            nearLight.range = nearRange;
            nearLight.spotAngle = nearSpotAngle;
            nearLight.innerSpotAngle = nearSpotAngle * 0.7f;

            // Khong tao bong cho luong sang phu
            nearLight.shadows = LightShadows.None;
        }

        private void UpdateHandMovement()
        {
            // Lay chuyen dong chuot
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Gioi han de tranh giat manh
            mouseX = Mathf.Clamp(mouseX, -5f, 5f);
            mouseY = Mathf.Clamp(mouseY, -5f, 5f);

            // Xoay den nguoc nhe theo huong chuot
            Quaternion swayRotation = Quaternion.Euler(
                mouseY * swayAmount,
                -mouseX * swayAmount,
                -mouseX * swayAmount * 0.5f
            );

            Quaternion targetRotation =
                Quaternion.Euler(holdRotation) * swayRotation;

            currentLocalRotation = Quaternion.Slerp(
                currentLocalRotation,
                targetRotation,
                1f - Mathf.Exp(-swaySmooth * Time.deltaTime)
            );

            // Do tre vi tri khi xoay camera
            Vector3 swayOffset = new Vector3(
                -mouseX * swayPositionAmount,
                -mouseY * swayPositionAmount,
                0f
            );

            // Kiem tra nhan vat co dang di chuyen
            bool isWalking = false;

            if (playerRoot != null)
            {
                Vector3 currentPosition = playerRoot.position;

                Vector3 movement =
                    currentPosition - lastPlayerPosition;

                movement.y = 0f;

                float movementSpeed =
                    movement.magnitude / Mathf.Max(
                        Time.deltaTime,
                        0.0001f
                    );

                isWalking = movementSpeed > 0.1f;

                lastPlayerPosition = currentPosition;
            }

            Vector3 bobOffset = Vector3.zero;

            if (isWalking)
            {
                bobTimer += Time.deltaTime * bobSpeed;

                bobOffset = new Vector3(
                    Mathf.Cos(bobTimer) * bobAmount,
                    Mathf.Sin(bobTimer * 2f) * bobAmount,
                    0f
                );
            }

            Vector3 targetPosition =
                holdPosition + swayOffset + bobOffset;

            currentLocalPosition = Vector3.Lerp(
                currentLocalPosition,
                targetPosition,
                1f - Mathf.Exp(-swaySmooth * Time.deltaTime)
            );

            heldFlashlight.transform.localPosition =
                currentLocalPosition;

            heldFlashlight.transform.localRotation =
                currentLocalRotation;
        }
    }
}