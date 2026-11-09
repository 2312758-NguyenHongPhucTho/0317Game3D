
namespace FpsHorrorKit
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.HighDefinition;

    public class InteractCameraSettings : MonoBehaviour
    {
        private Volume volume;
        private DepthOfField depthOfField;
        private float startFocusDistance;

        public static InteractCameraSettings Instance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            FindDepthOfField();
        }

        private void FindDepthOfField()
        {
            Volume[] volumes =
                FindObjectsByType<Volume>(
                    FindObjectsSortMode.None
                );

            foreach (Volume v in volumes)
            {
                if (v.profile != null &&
                    v.profile.TryGet<DepthOfField>(
                        out DepthOfField dof))
                {
                    volume = v;
                    depthOfField = dof;

                    startFocusDistance =
                        depthOfField.focusDistance.value;

                    Debug.Log(
                        "Depth of Field da duoc tim thay!"
                    );

                    return;
                }
            }

            // Khong tim thay thi bo qua,
            // khong gay loi cho he thong tuong tac.
            Debug.LogWarning(
                "Khong tim thay Depth of Field. " +
                "Game van co the tiep tuc hoat dong."
            );
        }

        public void Interacting(
            float focusDistanceWhenInspecting = 0.5f)
        {
            if (depthOfField == null)
                return;

            depthOfField.focusDistance.value =
                focusDistanceWhenInspecting;
        }

        public void NotInteracting()
        {
            if (depthOfField == null)
                return;

            depthOfField.focusDistance.value =
                startFocusDistance;
        }

        public void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void HideCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
