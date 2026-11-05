using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FpsHorrorKit
{
    /// <summary>One-shot FPS opening interaction for the main door.</summary>
    public sealed class MainDoorOpeningSequence : MonoBehaviour, IInteractable
    {
        private enum OpeningState
        {
            WaitingForPlayer, Knocking, WaitingForLam, DoorOpening, DialogueCutscene,
            FadingOut, TeleportingToBed, WakingUp, Finished, Failed, Cancelled
        }

        [Header("References")]
        [SerializeField] private DoorSystem door;
        [SerializeField] private GameObject lamActorSource;
        [SerializeField] private Transform lamSpawnPoint;
        [SerializeField] private Transform bedSpawn;
        [SerializeField] private FpsController fpsController;
        [SerializeField] private PlayerInteract playerInteract;
        [SerializeField] private FpsAssetsInputs inputs;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip knockClip;
        [SerializeField] private AudioClip[] dialogueAudio;
        [SerializeField] private TMP_FontAsset vietnameseFont;

        [Header("Timing")]
        [SerializeField, Min(1)] private int knockCount = 3;
        [SerializeField, Min(.05f)] private float knockGap = .35f;
        [SerializeField, Min(0f)] private float afterKnockDelay = .7f;
        [SerializeField, Min(.1f)] private float minimumLineSeconds = 2.5f;
        [SerializeField, Min(1f)] private float readingCharactersPerSecond = 18f;
        [SerializeField, Min(.1f)] private float doorOpenTimeout = 5f;
        [SerializeField, Min(.05f)] private float fadeOutSeconds = .45f;
        [SerializeField, Min(0f)] private float blackScreenSeconds = 1.5f;
        [SerializeField, Min(.05f)] private float fadeInSeconds = .6f;
        [SerializeField, Min(.1f)] private float skipHoldSeconds = 1.5f;

        // 13 câu thoại cố định trong code để Unity không dùng lại mảng 12 câu cũ
        // đã được serialize trong Scene. dialogueAudio[0..12] tương ứng từng câu.
        private static readonly string[] lines =
        {
            "Ông Lâm: Ai đấy? Giờ này còn ai đến đây nữa?",
            "Người chơi: Dạ... cháu xin lỗi vì làm phiền. Cháu bị lạc trong rừng. Trời mưa lớn quá, chú cho cháu trú nhờ một lát được không ạ?",
            "Ông Lâm: Lạc trong rừng à? Giờ này sao cậu lại ở ngoài đó một mình?",
            "Người chơi: Cháu cũng không biết nữa... Điện thoại thì hết pin, đường lại tối. Cháu đi mãi mới thấy có ánh đèn ở đây.",
            "Ông Lâm: Ừm... Rừng này ban đêm không an toàn đâu. Nhất là lúc trời mưa thế này.",
            "Người chơi: Dạ... vậy chú cho cháu ở nhờ đến sáng được không ạ? Sáng mai cháu sẽ tìm đường về ngay.",
            "Ông Lâm: Thôi được rồi. Vào đi, đứng ngoài đó mãi lại cảm lạnh.",
            "Người chơi: Dạ, cháu cảm ơn chú nhiều lắm ạ.",
            "Ông Lâm: À, mà này... Có một chuyện tôi phải dặn trước.",
            "Người chơi: Dạ? Chuyện gì vậy chú?",
            "Ông Lâm: Đêm nay, nếu có nghe thấy tiếng động gì trong nhà... thì cứ ở yên trong phòng. Đừng ra ngoài. Nhớ chưa?",
            "Người chơi: Dạ... cháu nhớ rồi ạ.",
            "Ông Lâm: Ừ. Đi theo tôi."
        };

        private OpeningState state = OpeningState.WaitingForPlayer;
        private Coroutine sequence;
        private bool controlsSaved;
        private bool previousFpsInteracting;
        private bool previousSendRaycast;
        private GameObject previousHighlightObject;
        private bool previousHighlightWasActive;
        private GameObject lamInstance;
        private GameObject subtitleRoot;
        private TextMeshProUGUI speakerText;
        private TextMeshProUGUI dialogueText;
        private Image fadeImage;
        private GameObject skipRoot;
        private Image skipProgress;
        private float skipHeldSeconds;
        private bool skipArmed = true;
        private bool skipTriggered;

        private void Update()
        {
            UpdateSkipHold();
        }

        public void Interact()
        {
            if (state != OpeningState.WaitingForPlayer || sequence != null) return;
            if (door == null)
            {
                Fail("[03:17] Opening needs a DoorSystem reference.");
                return;
            }
            // A moving door is a transient condition; keep the opening available for a later E press.
            if (door.IsMoving) return;
            sequence = StartCoroutine(RunSequence());
        }

        public void Highlight()
        {
            if (state == OpeningState.WaitingForPlayer && PlayerInteract.Instance != null)
                PlayerInteract.Instance.ChangeInteractText("Nhấn [E] để gõ cửa");
        }

        public void HoldInteract() { }
        public void UnHighlight() { }

        private IEnumerator RunSequence()
        {
            SaveAndLockControls();
            ClearInteractInput();
            state = OpeningState.Knocking;
            if (audioSource == null || knockClip == null)
                Debug.LogWarning("[03:17] Opening knock AudioSource or AudioClip is unassigned; continuing silently.");
            for (int i = 0; i < knockCount; i++)
            {
                Play(knockClip);
                if (i + 1 < knockCount) yield return WaitRealtime(knockGap);
            }

            state = OpeningState.WaitingForLam;
            yield return WaitRealtime(afterKnockDelay);
            if (lines == null || lines.Length == 0)
            {
                Fail("[03:17] Opening dialogue has no lines.");
                yield break;
            }
            state = OpeningState.DoorOpening;
            if (!door.IsOpen)
            {
                door.Interact();
                float elapsed = 0f;
                while ((door.IsMoving || !door.IsOpen) && elapsed < doorOpenTimeout)
                {
                    if (RoomEightMainMenu.IsPauseActive) { yield return null; continue; }
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
                if (!door.IsOpen || door.IsMoving)
                {
                    Fail("[03:17] Main door did not finish opening before timeout.");
                    yield break;
                }
            }

            SpawnLam();
            state = OpeningState.DialogueCutscene;
            for (int i = 0; i < lines.Length; i++) yield return ShowLine(i, true);
            SetSubtitle(string.Empty, string.Empty);

            state = OpeningState.FadingOut;
            yield return Fade(0f, 1f, fadeOutSeconds);
            yield return WaitRealtime(blackScreenSeconds);

            state = OpeningState.TeleportingToBed;
            if (!TeleportToBed())
            {
                Fail("[03:17] Opening needs a BedSpawn reference before the wake-up transition.");
                yield break;
            }

            bool doorSecured = false;
            yield return CloseAndLockMainDoor(result => doorSecured = result);
            if (!doorSecured)
            {
                Fail("[03:17] Opening could not secure the main door before wake-up.");
                yield break;
            }

            state = OpeningState.WakingUp;
            yield return Fade(1f, 0f, fadeInSeconds);
            state = OpeningState.Finished;
            Complete();
        }

        private IEnumerator ShowLine(int index, bool allowSkip)
        {
            if (index < 0 || index >= lines.Length) yield break;
            string speaker;
            string content;
            SplitLine(lines[index], out speaker, out content);
            SetSubtitle(speaker, content);
            AudioClip voiceClip = dialogueAudio != null && index < dialogueAudio.Length ? dialogueAudio[index] : null;
            if (voiceClip != null) Play(voiceClip);
            while (inputs != null && inputs.interact) yield return null;

            float duration = Mathf.Max(minimumLineSeconds, content.Length / readingCharactersPerSecond);
            // Khi lồng tiếng, không tự chuyển phụ đề trước khi giọng đọc kết thúc.
            if (voiceClip != null) duration = Mathf.Max(duration, voiceClip.length + 0.25f);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (RoomEightMainMenu.IsPauseActive) { yield return null; continue; }
                elapsed += Time.unscaledDeltaTime;
                if (allowSkip && inputs != null && inputs.interact)
                {
                    ClearInteractInput();
                    // Dừng câu thoại đang phát trước khi chuyển sang câu tiếp theo.
                    if (voiceClip != null && audioSource != null) audioSource.Stop();
                    yield break;
                }
                yield return null;
            }
        }

        private void SaveAndLockControls()
        {
            if (controlsSaved) return;
            if (fpsController != null)
            {
                previousFpsInteracting = fpsController.isInteracting;
                fpsController.isInteracting = true;
            }
            if (playerInteract != null)
            {
                previousSendRaycast = playerInteract.sendRaycast;
                previousHighlightObject = playerInteract.higlightObject;
                previousHighlightWasActive = previousHighlightObject != null && previousHighlightObject.activeSelf;
                playerInteract.sendRaycast = false;
                HideInteractionUi();
            }
            controlsSaved = true;
            EnsureSubtitle();
        }

        private void RestoreControls()
        {
            if (!controlsSaved) return;
            if (fpsController != null) fpsController.isInteracting = previousFpsInteracting;
            if (playerInteract != null)
            {
                playerInteract.sendRaycast = previousSendRaycast;
                if (previousSendRaycast)
                    HideInteractionUi();
                else if (previousHighlightObject != null)
                    previousHighlightObject.SetActive(previousHighlightWasActive);
            }
            ClearInteractInput();
            controlsSaved = false;
        }

        private void HideInteractionUi()
        {
            if (playerInteract == null) return;
            if (playerInteract.higlightObject != null) playerInteract.higlightObject.SetActive(false);
            if (playerInteract.interactTextUI != null) playerInteract.interactTextUI.gameObject.SetActive(false);
            if (playerInteract.interactImageUI != null) playerInteract.interactImageUI.gameObject.SetActive(false);
        }

        private void SpawnLam()
        {
            if (lamActorSource == null || lamSpawnPoint == null)
            {
                Debug.LogWarning("[03:17] Opening Lam actor source or spawn point is unassigned; dialogue continues without actor.");
                return;
            }
            lamInstance = Instantiate(lamActorSource, lamSpawnPoint.position, lamSpawnPoint.rotation);
            lamInstance.name = "Lam_Opening_Runtime";
            lamInstance.SetActive(true);
            if (fpsController != null)
            {
                Vector3 facing = fpsController.transform.position - lamInstance.transform.position;
                facing.y = 0f;
                if (facing.sqrMagnitude > .0001f) lamInstance.transform.rotation = Quaternion.LookRotation(facing);
            }
            Animator animator = lamInstance.GetComponentInChildren<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null) animator.Play("Idle", 0, 0f);
            else Debug.LogWarning("[03:17] Opening Lam clone has no usable Animator; showing static visual.");
        }

        private void EnsureSubtitle()
        {
            if (subtitleRoot != null) return;
            subtitleRoot = new GameObject("MainDoorOpeningSubtitle", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            subtitleRoot.transform.SetParent(transform, false);
            Canvas canvas = subtitleRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 7900;
            CanvasScaler scaler = subtitleRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            fadeImage = CreateFadeImage(subtitleRoot.transform);
            speakerText = CreateText("Speaker", subtitleRoot.transform, new Vector2(.08f, .18f), new Vector2(.92f, .23f), 28f);
            dialogueText = CreateText("Dialogue", subtitleRoot.transform, new Vector2(.08f, .07f), new Vector2(.92f, .18f), 34f);
            CreateSkipUI(subtitleRoot.transform);
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, Vector2 min, Vector2 max, float size)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            if (vietnameseFont != null) text.font = vietnameseFont;
            text.fontSize = size;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Image CreateFadeImage(Transform parent)
        {
            GameObject fadeObject = new GameObject("Fade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fadeObject.transform.SetParent(parent, false);
            RectTransform rect = fadeObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = fadeObject.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = false;
            return image;
        }

        private void SetSubtitle(string speaker, string content)
        {
            if (speakerText != null) speakerText.text = speaker;
            if (dialogueText != null) dialogueText.text = content;
        }

        private IEnumerator Fade(float from, float to, float seconds)
        {
            if (fadeImage == null) yield break;
            fadeImage.transform.SetAsLastSibling();
            if (seconds <= 0f)
            {
                SetFade(to);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < seconds)
            {
                if (RoomEightMainMenu.IsPauseActive) { yield return null; continue; }
                elapsed += Time.unscaledDeltaTime;
                SetFade(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / seconds)));
                yield return null;
            }
            SetFade(to);
        }

        private void SetFade(float alpha)
        {
            if (fadeImage == null) return;
            Color color = fadeImage.color;
            color.a = Mathf.Clamp01(alpha);
            fadeImage.color = color;
        }

        private void UpdateSkipHold()
        {
            if (RoomEightMainMenu.IsPauseActive) { SetSkipProgress(0f); return; }
            bool eligible = state == OpeningState.Knocking || state == OpeningState.WaitingForLam ||
                state == OpeningState.DoorOpening || state == OpeningState.DialogueCutscene ||
                state == OpeningState.FadingOut;
            if (skipRoot != null) skipRoot.SetActive(eligible && !skipTriggered);
            if (!eligible || skipTriggered) { skipHeldSeconds = 0f; return; }

            if (inputs != null) inputs.jump = false;
            var space = Keyboard.current != null ? Keyboard.current.spaceKey : null;
            if (space == null || !space.isPressed)
            {
                skipHeldSeconds = 0f;
                skipArmed = true;
                skipTriggered = false;
                SetSkipProgress(0f);
                return;
            }
            if (!skipArmed) return;

            skipHeldSeconds += Time.unscaledDeltaTime;
            SetSkipProgress(skipHeldSeconds / Mathf.Max(.1f, skipHoldSeconds));
            if (skipHeldSeconds < skipHoldSeconds) return;

            skipTriggered = true;
            skipArmed = false;
            if (sequence != null) StopCoroutine(sequence);
            sequence = StartCoroutine(SkipToLoopOne());
        }

        private IEnumerator SkipToLoopOne()
        {
            SaveAndLockControls();
            if (audioSource != null) audioSource.Stop();
            SetSubtitle(string.Empty, string.Empty);
            if (skipRoot != null) skipRoot.SetActive(false);
            SetFade(1f);

            state = OpeningState.TeleportingToBed;
            if (!TeleportToBed())
            {
                Fail("[03:17] Opening skip needs a BedSpawn reference.");
                yield break;
            }

            bool doorSecured = false;
            yield return CloseAndLockMainDoor(result => doorSecured = result);
            if (!doorSecured)
            {
                Fail("[03:17] Opening skip could not secure the main door.");
                yield break;
            }

            state = OpeningState.WakingUp;
            yield return Fade(1f, 0f, fadeInSeconds);
            state = OpeningState.Finished;
            Complete();
        }

        private void CreateSkipUI(Transform parent)
        {
            skipRoot = new GameObject("SkipHint", typeof(RectTransform), typeof(Image));
            skipRoot.transform.SetParent(parent, false);
            RectTransform rootRect = skipRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.pivot = new Vector2(1f, 1f);
            rootRect.sizeDelta = new Vector2(280f, 66f);
            rootRect.anchoredPosition = new Vector2(-28f, -28f);
            Image background = skipRoot.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, .55f);
            background.raycastTarget = false;
            TextMeshProUGUI label = CreateText("Label", skipRoot.transform, new Vector2(0f, .42f), new Vector2(1f, 1f), 20f);
            label.text = "Giữ [SPACE] để bỏ qua";
            GameObject progressObject = new GameObject("Progress", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            progressObject.transform.SetParent(skipRoot.transform, false);
            RectTransform progressRect = progressObject.GetComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(.08f, .14f);
            progressRect.anchorMax = new Vector2(.92f, .30f);
            progressRect.offsetMin = Vector2.zero;
            progressRect.offsetMax = Vector2.zero;
            skipProgress = progressObject.GetComponent<Image>();
            skipProgress.type = Image.Type.Filled;
            skipProgress.fillMethod = Image.FillMethod.Horizontal;
            skipProgress.fillAmount = 0f;
            skipProgress.color = new Color(.85f, .16f, .12f, 1f);
            skipProgress.raycastTarget = false;
            skipRoot.SetActive(false);
        }

        private void SetSkipProgress(float value)
        {
            if (skipProgress != null) skipProgress.fillAmount = Mathf.Clamp01(value);
        }

        private bool TeleportToBed()
        {
            if (bedSpawn == null || fpsController == null) return false;
            CharacterController controller = fpsController.GetComponent<CharacterController>();
            bool controllerWasEnabled = controller != null && controller.enabled;
            if (controller != null) controller.enabled = false;
            fpsController.transform.SetPositionAndRotation(bedSpawn.position, bedSpawn.rotation);
            if (controller != null) controller.enabled = controllerWasEnabled;
            return true;
        }

        private IEnumerator CloseAndLockMainDoor(System.Action<bool> completed)
        {
            if (door == null)
            {
                Debug.LogWarning("[03:17] Opening cannot close the main door: DoorSystem is unassigned.");
                completed?.Invoke(false);
                yield break;
            }

            float elapsed = 0f;
            while (door.IsMoving && elapsed < doorOpenTimeout)
            {
                if (RoomEightMainMenu.IsPauseActive) { yield return null; continue; }
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            if (door.IsMoving)
            {
                Debug.LogWarning("[03:17] Opening could not close the main door: it did not settle before timeout.");
                completed?.Invoke(false);
                yield break;
            }

            if (door.IsOpen)
            {
                if (door.isLocked)
                {
                    Debug.LogWarning("[03:17] Opening could not close the main door: it is already locked while open.");
                    completed?.Invoke(false);
                    yield break;
                }

                door.Interact();
                // DoorSystem changes IsOpen before its coroutine marks IsMoving, so wait one frame first.
                yield return null;
                elapsed = 0f;
                while (door.IsMoving && elapsed < doorOpenTimeout)
                {
                    if (RoomEightMainMenu.IsPauseActive) { yield return null; continue; }
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            if (door.IsMoving || door.IsOpen)
            {
                Debug.LogWarning("[03:17] Opening could not verify that the main door is fully closed; leaving it unlocked.");
                completed?.Invoke(false);
                yield break;
            }

            door.isLocked = true;
            completed?.Invoke(true);
        }

        private static void SplitLine(string line, out string speaker, out string content)
        {
            int split = string.IsNullOrEmpty(line) ? -1 : line.IndexOf(':');
            speaker = split < 0 ? string.Empty : line.Substring(0, split);
            content = split < 0 ? line ?? string.Empty : line.Substring(split + 1).TrimStart();
        }

        private static IEnumerator WaitRealtime(float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                if (!RoomEightMainMenu.IsPauseActive) elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void Play(AudioClip clip) { if (audioSource != null && clip != null) audioSource.PlayOneShot(clip); }
        private void ClearInteractInput() { if (inputs != null) inputs.interact = false; }

        private void Complete()
        {
            RestoreControls();
            CleanupRuntime();
            sequence = null;
        }

        private void CleanupRuntime()
        {
            if (subtitleRoot != null) Destroy(subtitleRoot);
            if (lamInstance != null) Destroy(lamInstance);
            subtitleRoot = null;
            lamInstance = null;
            speakerText = null;
            dialogueText = null;
            fadeImage = null;
            skipRoot = null;
            skipProgress = null;
        }

        private void Fail(string message)
        {
            Debug.LogWarning(message);
            state = OpeningState.Failed;
            SetFade(0f);
            SetSubtitle(string.Empty, string.Empty);
            RestoreControls();
            SetFade(0f);
            sequence = null;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            if (state != OpeningState.Finished) state = OpeningState.Cancelled;
            RestoreControls();
            SetFade(0f);
            CleanupRuntime();
            sequence = null;
        }
    }
}
