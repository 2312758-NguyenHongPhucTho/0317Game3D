using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FpsHorrorKit
{
    /// <summary>
    /// One-scene story sequence:
    /// Ritual completed -> Lam dialogue -> door locks -> free-movement chase
    /// -> 3 hits -> blood/fall/blink/blackout -> respawn at bed -> Loop 2
    /// -> returning to ritual room plays orbit camera outro.
    /// Connect RitualManager.onRitualCompleted to PlayCutscene().
    /// </summary>
    public sealed class LamAttackCutscene : MonoBehaviour
    {
        private enum Stage { Waiting, Introduction, Chase, Dying, Loop2, Ending, Complete }
        private Stage stage = Stage.Waiting;

        [Header("Player and Cameras")]
        [SerializeField] private Transform playerRoot;
        [SerializeField] private CharacterController playerCharacterController;
        [SerializeField] private PlayerHealth0317 playerHealth;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Camera lamCloseupCamera;
        [SerializeField] private Camera endingOrbitCamera;
        [Tooltip("Player movement, look and interaction behaviours. Do NOT include this script.")]
        [SerializeField] private Behaviour[] playerControlScripts;

        [Header("Lam (temporary DOE Ghost clone)")]
        [SerializeField] private GameObject lamActor;
        [SerializeField] private Transform lamDoorSpawn;
        [Tooltip("Separate seated/static Lam model for Loop 2; do not use the same actor.")]
        [SerializeField] private GameObject seatedLamVisual;
        [SerializeField] private Transform lamSeatedPoint;
        [SerializeField] private float actorFacingOffsetY = 0f;
        [SerializeField] private Animator lamAnimator;
        [Tooltip("Optional attack trigger, leave empty if DOE Ghost has no suitable trigger.")]
        [SerializeField] private string attackTrigger = "";

        [Header("Door and Loop 2")]
        [SerializeField] private RitualRoomDoorLock ritualDoor;
        [SerializeField] private Transform bedSpawn;
        [Tooltip("Static corpse mesh/prop only, NOT a duplicate active Player controller.")]
        [SerializeField] private GameObject corpseVisual;
        [SerializeField] private Loop2ReturnTrigger loop2ReturnTrigger;

        [Header("Chase and 3 Hits")]
        [SerializeField, Min(0.1f)] private float chaseSpeed = 2.1f;
        [SerializeField, Min(0.2f)] private float hitDistance = 1.5f;
        [SerializeField, Min(0.1f)] private float hitCooldown = 1.65f;
        [SerializeField] private int hitsToKill = 3;
        [SerializeField, Min(0.1f)] private float hitShakeDuration = 0.3f;

        [Header("Death and Respawn")]
        [SerializeField, Min(0.1f)] private float fallDuration = 1.3f;
        [SerializeField] private float fallCameraDrop = 0.65f;
        [SerializeField, Min(0.1f)] private float blackoutFadeDuration = 0.65f;
        [SerializeField, Min(0f)] private float blackHoldSeconds = 3f;
        [SerializeField, Min(0.1f)] private float wakeFadeDuration = 1f;
        [Tooltip("Optional blood splatter PNG sprite with transparent background.")]
        [SerializeField] private Sprite bloodSplatterSprite;

        [Header("Introduction")]
        [TextArea(2, 3)] [SerializeField] private string line1 = "CHÁU ĐANG LÀM GÌ ĐẤY?!";
        [TextArea(2, 3)] [SerializeField] private string line2 = "Mày đã phá hủy tất cả những gì tao gây dựng!";
        [TextArea(2, 3)] [SerializeField] private string chaseLine = "Mày chính là vật hiến tế cuối cùng!";
        [SerializeField, Min(0.1f)] private float introLine1Seconds = 2.8f;
        [SerializeField, Min(0.1f)] private float introLine2Seconds = 3.2f;

        [Header("Loop 2 Ending Orbit")]
        [SerializeField] private Transform orbitCenter;
        [SerializeField, Min(0.5f)] private float orbitRadius = 3.2f;
        [SerializeField] private float orbitHeight = 1.8f;
        [SerializeField] private float orbitStartingAngle = 10f;
        [SerializeField] private float orbitTotalDegrees = 270f;
        [SerializeField, Min(1f)] private float orbitDuration = 18f;
        [SerializeField, Min(0f)] private float observeRoomSeconds = 3f;
        [TextArea(2, 3)] [SerializeField] private string endingLine1 = "Hai linh hồn đã thoát...";
        [TextArea(2, 3)] [SerializeField] private string endingLine2 = "Nhưng mày thì không...";
        [TextArea(2, 3)] [SerializeField] private string endingLine3 = "Mãi mãi... mày sẽ thuộc về căn nhà này.";
        [SerializeField] private TMP_FontAsset vietnameseFont;
        [SerializeField] private AudioSource endingMusic;
        [SerializeField] private GameObject optionalEndingPanel;

        [Header("Optional Sounds")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip doorSlamSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip fallSound;
        [SerializeField] private AudioClip clock0317Sound;

        [Header("Hooks (existing Blink system)")]
        [Tooltip("Connect the EXISTING blink component's public method here, if available.")]
        public UnityEvent onBlinkRequested = new UnityEvent();
        public UnityEvent onLoop2Started = new UnityEvent();
        public UnityEvent onEndingStarted = new UnityEvent();
        public UnityEvent onEndingFinished = new UnityEvent();

        private bool[] previousScriptStates;
        private bool controlsSaved;
        private Vector3 cameraOriginalLocalPosition;
        private Quaternion cameraOriginalLocalRotation;
        private Vector3 cameraBedLocalPosition;
        private Quaternion cameraBedLocalRotation;
        private int hitCount;
        private float nextHitTime;
        private bool hitInProgress;
        private GameObject uiRoot;
        private Image bloodImage;
        private Image blackImage;
        private TextMeshProUGUI subtitleText;
        private TextMeshProUGUI endingTitleText;

        public bool IsLoop2Ready { get { return stage == Stage.Loop2; } }

        private void Awake()
        {
            if (gameplayCamera == null) gameplayCamera = Camera.main;
            if (playerRoot == null && gameplayCamera != null)
                playerRoot = gameplayCamera.transform.root;
            if (playerCharacterController == null && playerRoot != null)
                playerCharacterController = playerRoot.GetComponent<CharacterController>();
            if (playerHealth == null && playerRoot != null)
                playerHealth = playerRoot.GetComponent<PlayerHealth0317>();

            if (gameplayCamera != null)
            {
                cameraBedLocalPosition = gameplayCamera.transform.localPosition;
                cameraBedLocalRotation = gameplayCamera.transform.localRotation;
            }
            if (lamCloseupCamera != null) lamCloseupCamera.enabled = false;
            if (endingOrbitCamera != null) endingOrbitCamera.enabled = false;
            if (lamActor != null) lamActor.SetActive(false);
            if (seatedLamVisual != null) seatedLamVisual.SetActive(false);
            if (corpseVisual != null) corpseVisual.SetActive(false);
            if (optionalEndingPanel != null) optionalEndingPanel.SetActive(false);
            BuildUI();
        }

        [ContextMenu("TEST: Start Lam Cutscene (Play Mode)")]
        private void TestCutscene()
        {
            if (Application.isPlaying) PlayCutscene();
        }

        public void PlayCutscene()
        {
            if (stage != Stage.Waiting) return;
            if (playerRoot == null || gameplayCamera == null || lamActor == null || lamCloseupCamera == null)
            {
                Debug.LogError("[03:17] Missing Player, Gameplay Camera, Lam Actor or Closeup Camera.");
                return;
            }
            if (ritualDoor == null)
                Debug.LogWarning("[03:17] Door Lock unassigned; escape door may stay open.");
            if (bedSpawn == null)
                Debug.LogWarning("[03:17] Bed Spawn missing; respawn will stay at death position.");

            stage = Stage.Introduction;
            SaveAndDisableControls();
            ritualDoor?.LockDoor();
            if (lamDoorSpawn != null)
                lamActor.transform.SetPositionAndRotation(lamDoorSpawn.position, lamDoorSpawn.rotation);
            lamActor.SetActive(true);
            FaceLamAtPlayer();
            PlaySfx(doorSlamSound);
            StartCoroutine(IntroductionRoutine());
        }

        private IEnumerator IntroductionRoutine()
        {
            SwitchCamera(lamCloseupCamera);
            SetSubtitle(line1);
            yield return new WaitForSecondsRealtime(introLine1Seconds);
            SetSubtitle(line2);
            yield return new WaitForSecondsRealtime(introLine2Seconds);
            SwitchCamera(gameplayCamera);
            SetSubtitle(chaseLine);
            RestoreControls();
            stage = Stage.Chase;
            nextHitTime = Time.unscaledTime + 0.5f;
            yield return new WaitForSecondsRealtime(2f);
            if (stage == Stage.Chase) SetSubtitle("");
        }

        private void Update()
        {
            if (stage != Stage.Chase || lamActor == null || playerRoot == null) return;
            Vector3 target = playerRoot.position;
            target.y = lamActor.transform.position.y; // Demo flat-floor chase, no NavMesh.
            Vector3 delta = target - lamActor.transform.position;
            if (delta.sqrMagnitude > hitDistance * hitDistance)
            {
                lamActor.transform.position = Vector3.MoveTowards(
                    lamActor.transform.position, target, chaseSpeed * Time.deltaTime);
            }
            FaceLamAtPlayer();

            if (delta.sqrMagnitude <= hitDistance * hitDistance &&
                Time.unscaledTime >= nextHitTime && !hitInProgress)
            {
                nextHitTime = Time.unscaledTime + hitCooldown;
                StartCoroutine(HitRoutine());
            }
        }

        private IEnumerator HitRoutine()
        {
            hitInProgress = true;
            hitCount++;
            if (playerHealth != null)
                playerHealth.SetHealthForCutscene(hitCount == 1 ? 67 : hitCount == 2 ? 33 : 0);
            if (lamAnimator != null && !string.IsNullOrWhiteSpace(attackTrigger))
                lamAnimator.SetTrigger(attackTrigger);
            PlaySfx(hitSound);
            yield return ShakeCamera(hitShakeDuration, 0.08f);
            if (hitCount >= Mathf.Max(1, hitsToKill))
            {
                stage = Stage.Dying;
                SetBloodAlpha(0.93f);
                StartCoroutine(DeathRoutine());
            }
            else
            {
                SetBloodAlpha(Mathf.Min(0.55f, 0.18f * hitCount));
            }
            hitInProgress = false;
        }

        private IEnumerator ShakeCamera(float seconds, float magnitude)
        {
            Transform cam = gameplayCamera.transform;
            Vector3 origin = cam.localPosition;
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                cam.localPosition = origin + Random.insideUnitSphere * magnitude;
                yield return null;
            }
            cam.localPosition = origin;
        }

        private IEnumerator DeathRoutine()
        {
            SaveAndDisableControls();
            SetSubtitle("");
            PlaySfx(fallSound);
            cameraOriginalLocalPosition = gameplayCamera.transform.localPosition;
            cameraOriginalLocalRotation = gameplayCamera.transform.localRotation;
            Vector3 destinationPosition = cameraOriginalLocalPosition + Vector3.down * fallCameraDrop;
            Quaternion destinationRotation = cameraOriginalLocalRotation * Quaternion.Euler(58f, 0f, 65f);
            float elapsed = 0f;
            while (elapsed < fallDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / fallDuration));
                gameplayCamera.transform.localPosition = Vector3.Lerp(cameraOriginalLocalPosition, destinationPosition, t);
                gameplayCamera.transform.localRotation = Quaternion.Slerp(cameraOriginalLocalRotation, destinationRotation, t);
                yield return null;
            }

            // Link your project's EXISTING Blink component through this UnityEvent.
            onBlinkRequested?.Invoke();
            yield return FadeBlack(0f, 1f, blackoutFadeDuration);
            yield return new WaitForSecondsRealtime(blackHoldSeconds);

            // Reset only the player, NOT the whole scene: destroyed dolls stay destroyed.
            RespawnAtBedWhileBlack();
            PlaySfx(clock0317Sound);
            yield return FadeBlack(1f, 0f, wakeFadeDuration);
            SetBloodAlpha(0f);
            RestoreControls();
            stage = Stage.Loop2;
            onLoop2Started?.Invoke();
            Debug.Log("[03:17] Loop 2: player woke up at 03:17; dolls remain destroyed.");
        }

        private void RespawnAtBedWhileBlack()
        {
            if (playerCharacterController != null) playerCharacterController.enabled = false;
            if (bedSpawn != null)
                playerRoot.SetPositionAndRotation(bedSpawn.position, bedSpawn.rotation);
            gameplayCamera.transform.localPosition = cameraBedLocalPosition;
            gameplayCamera.transform.localRotation = cameraBedLocalRotation;
            if (playerCharacterController != null) playerCharacterController.enabled = true;
            playerHealth?.RestoreFullHealth();

            if (lamActor != null) lamActor.SetActive(false);
            if (seatedLamVisual != null)
            {
                if (lamSeatedPoint != null)
                    seatedLamVisual.transform.SetPositionAndRotation(lamSeatedPoint.position, lamSeatedPoint.rotation);
                seatedLamVisual.SetActive(true);
            }
            if (corpseVisual != null) corpseVisual.SetActive(true);
            ritualDoor?.OpenForLoop2();
            loop2ReturnTrigger?.Arm();
            SwitchCamera(gameplayCamera);
            SetSubtitle("03:17... Mình vẫn còn sống sao?");
            StartCoroutine(ClearSubtitleAfter(3f));
        }

        private IEnumerator ClearSubtitleAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            if (stage == Stage.Loop2) SetSubtitle("");
        }

        /// <summary>Called by Loop2ReturnTrigger when the player revisits the ritual room.</summary>
        public void StartLoop2Ending()
        {
            if (stage != Stage.Loop2) return;
            if (endingOrbitCamera == null || orbitCenter == null)
            {
                Debug.LogError("[03:17] Ending needs Ending Orbit Camera and Orbit Center.");
                return;
            }
            stage = Stage.Ending;
            SaveAndDisableControls();
            StartCoroutine(EndingRoutine());
        }

        private IEnumerator EndingRoutine()
        {
            // Let players see Lam at the altar and their own body first.
            SetSubtitle("Không... người nằm trong vòng tế... là mình sao?");
            if (observeRoomSeconds > 0f)
                yield return new WaitForSecondsRealtime(observeRoomSeconds);
            SetSubtitle("");
            SwitchCamera(endingOrbitCamera);
            onEndingStarted?.Invoke();
            if (endingMusic != null && endingMusic.clip != null)
            {
                endingMusic.loop = true;
                endingMusic.Play();
            }
            float elapsed = 0f;
            while (elapsed < orbitDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / orbitDuration);
                float degrees = orbitStartingAngle + orbitTotalDegrees * t;
                Vector3 center = orbitCenter.position;
                Vector3 offset = Quaternion.Euler(0f, degrees, 0f) * (Vector3.forward * orbitRadius);
                endingOrbitCamera.transform.position = center + offset + Vector3.up * orbitHeight;
                endingOrbitCamera.transform.LookAt(center + Vector3.up * 0.8f);

                if (t < 0.33f) SetSubtitle(endingLine1);
                else if (t < 0.66f) SetSubtitle(endingLine2);
                else SetSubtitle(endingLine3);
                yield return null;
            }
            SetSubtitle("");
            yield return FadeBlack(0f, 1f, 2f);
            if (endingMusic != null) endingMusic.Stop();
            if (optionalEndingPanel != null) optionalEndingPanel.SetActive(true);
            else if (endingTitleText != null) endingTitleText.gameObject.SetActive(true);
            stage = Stage.Complete;
            onEndingFinished?.Invoke();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void FaceLamAtPlayer()
        {
            Vector3 direction = playerRoot.position - lamActor.transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
                lamActor.transform.rotation = Quaternion.LookRotation(direction) *
                    Quaternion.Euler(0f, actorFacingOffsetY, 0f);
        }

        private void SwitchCamera(Camera activeCamera)
        {
            if (gameplayCamera != null) gameplayCamera.enabled = activeCamera == gameplayCamera;
            if (lamCloseupCamera != null) lamCloseupCamera.enabled = activeCamera == lamCloseupCamera;
            if (endingOrbitCamera != null) endingOrbitCamera.enabled = activeCamera == endingOrbitCamera;
        }

        private void SaveAndDisableControls()
        {
            if (controlsSaved) return;
            if (playerControlScripts == null) return;
            previousScriptStates = new bool[playerControlScripts.Length];
            for (int i = 0; i < playerControlScripts.Length; i++)
            {
                Behaviour behaviour = playerControlScripts[i];
                if (behaviour == null) continue;
                previousScriptStates[i] = behaviour.enabled;
                behaviour.enabled = false;
            }
            controlsSaved = true;
        }

        private void RestoreControls()
        {
            if (!controlsSaved || playerControlScripts == null) return;
            for (int i = 0; i < playerControlScripts.Length; i++)
            {
                if (playerControlScripts[i] != null)
                    playerControlScripts[i].enabled = previousScriptStates[i];
            }
            controlsSaved = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void PlaySfx(AudioClip clip)
        {
            if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
        }

        private IEnumerator FadeBlack(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration)));
                SetAlpha(blackImage, a);
                yield return null;
            }
            SetAlpha(blackImage, to);
        }

        private void SetBloodAlpha(float alpha) { SetAlpha(bloodImage, alpha); }
        private static void SetAlpha(Image img, float a)
        {
            if (img == null) return;
            Color c = img.color;
            c.a = Mathf.Clamp01(a);
            img.color = c;
        }
        private void SetSubtitle(string value)
        {
            if (subtitleText != null) subtitleText.text = value;
        }

        private void BuildUI()
        {
            uiRoot = new GameObject("0317_Cutscene_UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = uiRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 8000;
            CanvasScaler scaler = uiRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            RectTransform canvasTransform = uiRoot.GetComponent<RectTransform>();

            bloodImage = CreateFullScreenImage("Blood_Splatter", canvasTransform,
                bloodSplatterSprite, bloodSplatterSprite == null
                    ? new Color(0.6f, 0f, 0f, 0f)
                    : new Color(1f, 1f, 1f, 0f));

            blackImage = CreateFullScreenImage("Blackout", canvasTransform, null, Color.clear);
            blackImage.color = new Color(0, 0, 0, 0);

            GameObject subtitleGo = new GameObject("Dialogue", typeof(RectTransform), typeof(TextMeshProUGUI));
            subtitleGo.transform.SetParent(canvasTransform, false);
            RectTransform sr = subtitleGo.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0.08f, 0.07f);
            sr.anchorMax = new Vector2(0.92f, 0.20f);
            sr.offsetMin = Vector2.zero;
            sr.offsetMax = Vector2.zero;
            subtitleText = subtitleGo.GetComponent<TextMeshProUGUI>();
            if (vietnameseFont != null) subtitleText.font = vietnameseFont;
            subtitleText.alignment = TextAlignmentOptions.Center;
            subtitleText.fontSize = 36;
            subtitleText.color = new Color(1f, 0.89f, 0.80f);
            subtitleText.outlineWidth = 0.15f;
            subtitleText.raycastTarget = false;
            subtitleText.text = "";

            GameObject endGo = new GameObject("EndingTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            endGo.transform.SetParent(canvasTransform, false);
            RectTransform er = endGo.GetComponent<RectTransform>();
            er.anchorMin = Vector2.zero;
            er.anchorMax = Vector2.one;
            er.offsetMin = Vector2.zero;
            er.offsetMax = Vector2.zero;
            endingTitleText = endGo.GetComponent<TextMeshProUGUI>();
            if (vietnameseFont != null) endingTitleText.font = vietnameseFont;
            endingTitleText.fontSize = 66;
            endingTitleText.color = new Color(0.85f, 0.11f, 0.09f);
            endingTitleText.alignment = TextAlignmentOptions.Center;
            endingTitleText.text = "03:17\nVÒNG LẶP TẾ HỒN\n\nTHE END";
            endingTitleText.raycastTarget = false;
            endGo.SetActive(false);
        }

        private static Image CreateFullScreenImage(string name, Transform parent, Sprite sprite, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }
    }
}
