
namespace FpsHorrorKit
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;

    public class RitualManager : MonoBehaviour
    {
        public enum DollType
        {
            Daughter,
            Mother
        }

        [Header("Quest Conditions")]
        [Tooltip("True after playing the daughter's music box")]
        [SerializeField] private bool musicBoxPlayed;

        [Tooltip("True after using the mother's photograph")]
        [SerializeField] private bool motherPhotoUsed;

        [Header("Seal Status")]
        [SerializeField] private bool daughterSealBroken;
        [SerializeField] private bool motherSealBroken;

        [Header("Doll Status")]
        [SerializeField] private bool daughterDestroyed;
        [SerializeField] private bool motherDestroyed;

        [Header("Ending Settings")]
        [SerializeField] private GameObject endingPanel;

        [SerializeField] private float endingDelay = 1.5f;

        public UnityEvent onRitualCompleted = new UnityEvent();

        private bool _endingStarted;

        private void Awake()
        {
            if (endingPanel != null)
            {
                endingPanel.SetActive(false);
            }
        }

        // Called when player activates the music box
        public void MarkMusicBoxPlayed()
        {
            musicBoxPlayed = true;

            Debug.Log(
                "[Ritual] Music box played. Daughter seal can be removed."
            );
        }

        // Called when player uses the mother's photograph
        public void MarkMotherPhotoUsed()
        {
            motherPhotoUsed = true;

            Debug.Log(
                "[Ritual] Mother's photograph activated."
            );
        }

        // Check if player can remove the seal
        public bool CanBreakSeal(DollType dollType)
        {
            if (_endingStarted)
                return false;

            if (dollType == DollType.Daughter)
            {
                return musicBoxPlayed &&
                       !daughterSealBroken &&
                       !daughterDestroyed;
            }

            if (dollType == DollType.Mother)
            {
                return motherPhotoUsed &&
                       daughterDestroyed &&
                       !motherSealBroken &&
                       !motherDestroyed;
            }

            return false;
        }

        // Remove the seal after conditions are met
        public bool TryBreakSeal(DollType dollType)
        {
            if (!CanBreakSeal(dollType))
            {
                Debug.Log(
                    "[Ritual] Cannot remove seal yet: " + dollType
                );

                return false;
            }

            if (dollType == DollType.Daughter)
            {
                daughterSealBroken = true;

                Debug.Log(
                    "[Ritual] Daughter seal removed!"
                );
            }
            else
            {
                motherSealBroken = true;

                Debug.Log(
                    "[Ritual] Mother seal removed!"
                );
            }

            return true;
        }

        // Used by InspectSystem before destroying a doll
        public bool IsSealBroken(DollType dollType)
        {
            if (dollType == DollType.Daughter)
                return daughterSealBroken;

            return motherSealBroken;
        }

        // Called by InspectSystem when player destroys a doll
        public bool NotifyDollDestroyed(DollType dollType)
        {
            if (!IsSealBroken(dollType))
            {
                Debug.LogWarning(
                    "[Ritual] Cannot destroy doll while seal is active!"
                );

                return false;
            }

            if (dollType == DollType.Daughter)
            {
                if (daughterDestroyed)
                    return false;

                daughterDestroyed = true;

                Debug.Log(
                    "[Ritual] Daughter soul released!"
                );
            }
            else
            {
                if (motherDestroyed)
                    return false;

                motherDestroyed = true;

                Debug.Log(
                    "[Ritual] Mother soul released!"
                );
            }

            CheckRitualCompletion();

            return true;
        }

        private void CheckRitualCompletion()
        {
            if (_endingStarted)
                return;

            if (!daughterDestroyed || !motherDestroyed)
                return;

            _endingStarted = true;

            Debug.Log(
                "[Ritual] Both souls released. Ritual interrupted!"
            );

            StartCoroutine(CompleteRitual());
        }

        private IEnumerator CompleteRitual()
        {
            if (endingDelay > 0f)
            {
                yield return new WaitForSeconds(endingDelay);
            }

            // Invoke custom ending events
            onRitualCompleted?.Invoke();

            // Show ending screen if assigned
            if (endingPanel != null)
            {
                endingPanel.SetActive(true);
            }

            Debug.Log("[Ritual] GAME COMPLETED!");
        }
    }
}
