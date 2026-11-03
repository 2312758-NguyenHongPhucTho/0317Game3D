namespace FpsHorrorKit
{
    using System.Collections;
    using UnityEngine;

    public class MusicBoxInteract : MonoBehaviour, IInteractable
    {
        [Header("Lid Settings")]
        [SerializeField] private Transform lid;
        [SerializeField] private Transform hinge;
        [SerializeField] private Transform additionalLidPart;
        [SerializeField] private Vector3 openAxis = Vector3.forward;
        [SerializeField] private float openAngle = 100f;
        [SerializeField] private float openDuration = 1.5f;

        [Header("Music Settings")]
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioClip musicClip;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.4f;
        [SerializeField] private bool loopMusic = true;

        [Header("Optional Decoration")]
        [SerializeField] private Transform rotatingDecoration;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private Animator decorationAnimator;

        [Header("Ritual Settings")]
        [SerializeField] private RitualManager ritualManager;

        [Header("Interaction Text")]
        [SerializeField] private string openText = "Nhấn [E] để mở hộp nhạc";
        [SerializeField] private string closeText = "Nhấn [E] để đóng hộp nhạc";

        private bool isAnimating;
        private bool isOpen;
        private bool ritualMarked;
        private Vector3 closedLidPosition;
        private Quaternion closedLidRotation;
        private Vector3 openLidPosition;
        private Quaternion openLidRotation;
        private Vector3 closedAdditionalLidPosition;
        private Quaternion closedAdditionalLidRotation;
        private Vector3 openAdditionalLidPosition;
        private Quaternion openAdditionalLidRotation;

        private void Awake()
        {
            if (ritualManager == null)
            {
                ritualManager = FindAnyObjectByType<RitualManager>();
            }

            if (decorationAnimator == null)
            {
                decorationAnimator = GetComponent<Animator>();
            }

            if (decorationAnimator != null)
            {
                decorationAnimator.enabled = false;
            }

            InitializeLidPoses();

            if (musicAudioSource != null)
            {
                musicAudioSource.playOnAwake = false;
                musicAudioSource.loop = loopMusic;
                musicAudioSource.spatialBlend = 1f;
                musicAudioSource.volume = musicVolume;
            }
        }

        private void Update()
        {
            if (isOpen && !isAnimating && rotatingDecoration != null)
            {
                rotatingDecoration.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
            }
        }

        public void Interact()
        {
            if (isAnimating)
            {
                return;
            }

            StartCoroutine(SetMusicBoxOpen(!isOpen));
        }

        public void Highlight()
        {
            if (!isAnimating && PlayerInteract.Instance != null)
            {
                PlayerInteract.Instance.ChangeInteractText(isOpen ? closeText : openText);
            }
        }

        public void HoldInteract()
        {
        }

        public void UnHighlight()
        {
        }

        private void InitializeLidPoses()
        {
            if (lid == null || hinge == null)
            {
                return;
            }

            // The imported model's local-zero pose is open; +100 degrees around barre is closed.
            openLidPosition = Vector3.zero;
            openLidRotation = Quaternion.identity;
            CalculateRotatedPose(lid, openLidPosition, openLidRotation, out closedLidPosition, out closedLidRotation);
            lid.localPosition = closedLidPosition;
            lid.localRotation = closedLidRotation;

            if (additionalLidPart != null)
            {
                openAdditionalLidPosition = Vector3.zero;
                openAdditionalLidRotation = Quaternion.identity;
                CalculateRotatedPose(additionalLidPart, openAdditionalLidPosition, openAdditionalLidRotation, out closedAdditionalLidPosition, out closedAdditionalLidRotation);
                additionalLidPart.localPosition = closedAdditionalLidPosition;
                additionalLidPart.localRotation = closedAdditionalLidRotation;
            }
        }

        private IEnumerator SetMusicBoxOpen(bool opening)
        {
            isAnimating = true;

            if (!opening && musicAudioSource != null)
            {
                musicAudioSource.Stop();
            }

            if (!opening && decorationAnimator != null)
            {
                decorationAnimator.enabled = false;
            }

            if (lid != null && hinge != null)
            {
                float elapsed = 0f;

                while (elapsed < openDuration)
                {
                    elapsed += Time.deltaTime;
                    SetLidPose(opening ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / openDuration)) : Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(elapsed / openDuration)));
                    yield return null;
                }

                SetLidPose(opening ? 1f : 0f);
            }
            else
            {
                Debug.LogWarning("[Music Box] Lid or hinge is not assigned.", this);
            }

            if (opening && musicAudioSource != null && musicClip != null)
            {
                musicAudioSource.clip = musicClip;
                musicAudioSource.volume = musicVolume;
                musicAudioSource.loop = loopMusic;
                musicAudioSource.Play();
            }

            if (opening)
            {
                if (decorationAnimator != null)
                {
                    decorationAnimator.enabled = true;
                    decorationAnimator.Play(0, 0, 0f);
                }

                if (!ritualMarked)
                {
                    ritualManager?.MarkMusicBoxPlayed();
                    ritualMarked = true;
                }
            }

            isOpen = opening;
            isAnimating = false;
        }

        private void CalculateRotatedPose(Transform part, Vector3 basePosition, Quaternion baseRotation, out Vector3 rotatedPosition, out Quaternion rotatedRotation)
        {
            Transform parent = part.parent;
            Vector3 axis = hinge.TransformDirection(openAxis.sqrMagnitude > 0f ? openAxis.normalized : Vector3.forward);
            Quaternion rotation = Quaternion.AngleAxis(openAngle, axis);
            Vector3 baseWorldPosition = parent.TransformPoint(basePosition);
            Quaternion baseWorldRotation = parent.rotation * baseRotation;

            rotatedPosition = parent.InverseTransformPoint(hinge.position + rotation * (baseWorldPosition - hinge.position));
            rotatedRotation = Quaternion.Inverse(parent.rotation) * rotation * baseWorldRotation;
        }

        private void SetLidPose(float progress)
        {
            lid.localPosition = Vector3.Lerp(closedLidPosition, openLidPosition, progress);
            lid.localRotation = Quaternion.Slerp(closedLidRotation, openLidRotation, progress);

            if (additionalLidPart != null)
            {
                additionalLidPart.localPosition = Vector3.Lerp(closedAdditionalLidPosition, openAdditionalLidPosition, progress);
                additionalLidPart.localRotation = Quaternion.Slerp(closedAdditionalLidRotation, openAdditionalLidRotation, progress);
            }
        }
    }
}
