
namespace FpsHorrorKit
{
    using UnityEngine;

    [RequireComponent(typeof(BoxCollider))]
    public class FlashlightClueReveal : MonoBehaviour
    {
        [Header("Player Settings")]
        [Tooltip("Automatically finds FlashlightBeam_Main")]
        public Light playerFlashlight;

        [Tooltip("Main camera of player")]
        public Camera playerCamera;

        [Header("Hidden Clue")]
        [Tooltip("PNG GameObject shown when flashlight is aiming")]
        public GameObject clueVisual;

        [Tooltip("Maximum reveal distance")]
        public float revealDistance = 5f;

        [Header("Reveal Settings")]
        [Tooltip("Keep PNG visible after first discovery")]
        public bool stayVisibleOnceFound = false;

        [Header("Optional Sound")]
        public AudioSource discoverySound;

        private BoxCollider _clueCollider;

        private Flashlight0317Controller flashlightController;

        private bool _discovered;
        private bool _isVisible;

        private float _nextFlashlightSearchTime;

        // =====================================
        // AWAKE
        // =====================================

        private void Awake()
        {
            _clueCollider = GetComponent<BoxCollider>();

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (clueVisual == null)
            {
                Debug.LogWarning(
                    "[FlashlightClue] Clue Visual is missing!"
                );
            }

            if (clueVisual == gameObject)
            {
                Debug.LogError(
                    "[FlashlightClue] Clue Visual cannot be the parent!"
                );

                enabled = false;
                return;
            }

            // Hide PNG when game starts
            SetClueVisible(false);
        }

        // =====================================
        // UPDATE - WORKS IN ALL LOOPS
        // =====================================

        private void Update()
        {
            // Find flashlight after player picks it up
            FindPlayerFlashlight();

            // Find camera automatically
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (clueVisual == null)
                return;

            // No loop restrictions
            bool shouldShow = false;

            if (stayVisibleOnceFound && _discovered)
            {
                shouldShow = true;
            }
            else
            {
                shouldShow = IsFlashlightAimingAtClue();
            }

            // First time finding the clue
            if (shouldShow && !_discovered)
            {
                _discovered = true;

                Debug.Log(
                    "[FlashlightClue] Clue discovered: " +
                    gameObject.name
                );

                if (discoverySound != null)
                {
                    discoverySound.Play();
                }
            }

            // Show or hide PNG
            SetClueVisible(shouldShow);
        }

        // =====================================
        // FIND FLASHLIGHT AUTOMATICALLY
        // =====================================

        private void FindPlayerFlashlight()
        {
            if (playerFlashlight != null)
                return;

            // Search every 0.5 seconds
            if (Time.unscaledTime < _nextFlashlightSearchTime)
                return;

            _nextFlashlightSearchTime =
                Time.unscaledTime + 0.5f;

            if (flashlightController == null)
            {
                flashlightController =
                    FindFirstObjectByType<Flashlight0317Controller>();
            }

            if (flashlightController == null)
                return;

            // Search runtime-created lights
            Light[] lights =
                flashlightController.GetComponentsInChildren<Light>(
                    true
                );

            foreach (Light light in lights)
            {
                if (light.gameObject.name == "FlashlightBeam_Main")
                {
                    playerFlashlight = light;

                    Debug.Log(
                        "[FlashlightClue] Found FlashlightBeam_Main!"
                    );

                    return;
                }
            }
        }

        // =====================================
        // CHECK FLASHLIGHT
        // =====================================

        private bool IsFlashlightAimingAtClue()
        {
            if (playerFlashlight == null ||
                playerCamera == null ||
                _clueCollider == null)
            {
                return false;
            }

            // Flashlight must be enabled
            if (!playerFlashlight.enabled)
                return false;

            // Flashlight must be active
            if (!playerFlashlight.gameObject.activeInHierarchy)
                return false;

            // Flashlight must have intensity
            if (playerFlashlight.intensity <= 0.01f)
                return false;

            // Raycast from camera center
            Ray ray = new Ray(
                playerCamera.transform.position,
                playerCamera.transform.forward
            );

            RaycastHit hit;

            if (!Physics.Raycast(
                ray,
                out hit,
                revealDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            // Raycast must hit clue collider
            if (hit.collider != _clueCollider)
            {
                return false;
            }

            Vector3 directionToClue =
                hit.point - playerFlashlight.transform.position;

            float distanceToClue =
                directionToClue.magnitude;

            // Check light range
            if (playerFlashlight.type == LightType.Spot ||
                playerFlashlight.type == LightType.Point)
            {
                if (distanceToClue > playerFlashlight.range)
                {
                    return false;
                }
            }

            // Check spotlight direction
            if (playerFlashlight.type == LightType.Spot)
            {
                float angle = Vector3.Angle(
                    playerFlashlight.transform.forward,
                    directionToClue
                );

                float maxAngle =
                    playerFlashlight.spotAngle * 0.5f;

                if (angle > maxAngle)
                {
                    return false;
                }
            }

            return true;
        }

        // =====================================
        // SHOW / HIDE PNG
        // =====================================

        private void SetClueVisible(bool visible)
        {
            if (clueVisual == null)
                return;

            if (_isVisible == visible &&
                clueVisual.activeSelf == visible)
            {
                return;
            }

            _isVisible = visible;

            clueVisual.SetActive(visible);
        }

        // =====================================
        // COMPATIBILITY WITH OLD LOOPMANAGER
        // =====================================

        // No longer needs unlocking.
        // Clue is available in every loop.
        public void UnlockClue()
        {
            // Already available in all loops
        }

        // Kept so an old LoopManager can call it
        // without disabling the clue.
        public void LockClue()
        {
            // No loop restriction
        }

        // =====================================
        // RESET DISCOVERY (OPTIONAL)
        // =====================================

        public void ResetDiscovery()
        {
            _discovered = false;
            SetClueVisible(false);
        }

        // =====================================
        // CLEANUP
        // =====================================

        private void OnDisable()
        {
            SetClueVisible(false);
        }
    }
}
