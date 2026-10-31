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
        [SerializeField] private float mainIntensity = 80f;
        [SerializeField] private float mainRange = 20f;
        [SerializeField] private float mainSpotAngle = 55f;
        [SerializeField] private float mainInnerAngle = 35f;

        [Header("Near Light - Sang Gan")]
        [SerializeField] private float nearIntensity = 0.5f;
        [SerializeField] private float nearRange = 1.5f;
        [SerializeField] private float nearSpotAngle = 45f;

        [Header("Mau anh sang vang")]
        [SerializeField]
        private Color mainLightColor =
            new Color(1f, 0.84f, 0.55f);

        [SerializeField]
        private Color nearLightColor =
            new Color(1f, 0.80f, 0.48f);

        [Header("Hieu ung den chap chon")]
        [SerializeField] private bool enableFlicker = true;

        [SerializeField, Range(0f, 1f)]
        private float flickerAmount = 0.12f;

        [SerializeField] private float flickerSpeed = 5f;

        [Header("Hand Sway")]
        [SerializeField] private float swayAmount = 2f;
        [SerializeField] private float swayPositionAmount = 0.012f;
        [SerializeField] private float swaySmooth = 10f;

        [Header("Walking Bob")]
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
            // Nhan R de bat/tat den
            if (hasFlashlight && Input.GetKeyDown(toggleKey))
            {
                SetEquipped(!isEquipped);
            }

            // Cap nhat anh sang chap chon
            UpdateFlicker();
        }

        private void LateUpdate()
        {
            if (heldFlashlight == null || !isEquipped)
                return;

            UpdateHandMovement();
        }

        public void GiveFlashlight(GameObject flashlightModel)
        {
            if (flashlightModel == null || hasFlashlight)
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

            currentLocalRotation =
                Quaternion.Euler(holdRotation);

            // Tat Collider
            foreach (Collider collider in
                heldFlashlight.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            // Xoa script nhat den
            Flashlight0317Pickup pickup =
                heldFlashlight.GetComponent<Flashlight0317Pickup>();

            if (pickup != null)
                Destroy(pickup);

            // Tat vat ly khi cam den
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
            GameObject beam =
                new GameObject("FlashlightBeam_Main");

            beam.transform.SetParent(
                heldFlashlight.transform,
                false
            );

            beam.transform.localPosition =
                new Vector3(0f, 0f, 0.46f);

            beam.transform.localRotation =
                Quaternion.identity;

            beamLight = beam.AddComponent<Light>();

            beamLight.type = LightType.Spot;

            // Mau vang am
            beamLight.color = mainLightColor;

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

            nearBeam.transform.localRotation =
                Quaternion.identity;

            nearLight = nearBeam.AddComponent<Light>();

            nearLight.type = LightType.Spot;

            // Mau vang gan
            nearLight.color = nearLightColor;

            nearLight.intensity = nearIntensity;
            nearLight.range = nearRange;

            nearLight.spotAngle = nearSpotAngle;

            nearLight.innerSpotAngle =
                nearSpotAngle * 0.7f;

            nearLight.shadows = LightShadows.None;
        }

        private void UpdateFlicker()
        {
            if (beamLight == null || !isEquipped)
                return;

            if (!enableFlicker)
            {
                beamLight.intensity = mainIntensity;

                if (nearLight != null)
                    nearLight.intensity = nearIntensity;

                return;
            }

            // Tao dao dong anh sang muot
            float noise = Mathf.PerlinNoise(
                Time.time * flickerSpeed,
                0.5f
            );

            // Anh sang chi giam nhe, khong tat han
            float multiplier = Mathf.Lerp(
                1f - flickerAmount,
                1f,
                noise
            );

            beamLight.intensity =
                mainIntensity * multiplier;

            if (nearLight != null)
            {
                nearLight.intensity =
                    nearIntensity * multiplier;
            }
        }

        private void UpdateHandMovement()
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            mouseX = Mathf.Clamp(mouseX, -5f, 5f);
            mouseY = Mathf.Clamp(mouseY, -5f, 5f);

            // Den tre nhe khi xoay chuot
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

            Vector3 swayOffset = new Vector3(
                -mouseX * swayPositionAmount,
                -mouseY * swayPositionAmount,
                0f
            );

            bool isWalking = false;

            if (playerRoot != null)
            {
                Vector3 currentPosition = playerRoot.position;

                Vector3 movement =
                    currentPosition - lastPlayerPosition;

                movement.y = 0f;

                float movementSpeed =
                    movement.magnitude /
                    Mathf.Max(Time.deltaTime, 0.0001f);

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