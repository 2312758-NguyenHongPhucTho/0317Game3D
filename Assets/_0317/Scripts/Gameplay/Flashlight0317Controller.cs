using UnityEngine;

namespace FpsHorrorKit
{
    public class Flashlight0317Controller : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.R;
        [SerializeField] private bool hasFlashlight;
        [SerializeField] private bool isEquipped;

        private GameObject heldFlashlight;
        private Light beamLight;

        private void Update()
        {
            if (hasFlashlight && Input.GetKeyDown(toggleKey))
                SetEquipped(!isEquipped);
        }

        public void GiveFlashlight(GameObject flashlightModel)
        {
            hasFlashlight = true;
            AttachFlashlightModel(flashlightModel);
            SetEquipped(true);
            InteractMessageScript.Instance?.ShowMessage("giấu đèn đi. Nhấn[R]");
        }

        private void SetEquipped(bool equipped)
        {
            isEquipped = equipped;
            if (heldFlashlight != null)
                heldFlashlight.SetActive(equipped);
        }

        // Reuse the pickup model so the held flashlight keeps its exact materials.
        private void AttachFlashlightModel(GameObject flashlightModel)
        {
            if (flashlightModel == null)
                return;

            heldFlashlight = flashlightModel;
            heldFlashlight.name = "HeldFlashlight_0317";
            heldFlashlight.transform.SetParent(transform, false);
            heldFlashlight.transform.localPosition = new Vector3(0.30f, -0.28f, 0.58f);
            heldFlashlight.transform.localRotation = Quaternion.Euler(5f, 0f, 0f);

            foreach (Collider collider in heldFlashlight.GetComponentsInChildren<Collider>())
                collider.enabled = false;

            Flashlight0317Pickup pickup = heldFlashlight.GetComponent<Flashlight0317Pickup>();
            if (pickup != null)
                Destroy(pickup);

            GameObject beam = new GameObject("FlashlightBeam");
            beam.transform.SetParent(heldFlashlight.transform, false);
            beam.transform.localPosition = new Vector3(0f, 0f, 0.46f);
            beamLight = beam.AddComponent<Light>();
            beamLight.type = LightType.Spot;
            beamLight.color = new Color(1f, 0.88f, 0.66f);
            beamLight.intensity = 900f;
            beamLight.range = 16f;
            beamLight.spotAngle = 42f;
            beamLight.shadows = LightShadows.Soft;

        }
    }

}
