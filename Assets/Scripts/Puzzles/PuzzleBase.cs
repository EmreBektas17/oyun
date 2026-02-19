using UnityEngine;
using PuzzleGame.Core;

namespace PuzzleGame.Puzzles
{
    /// <summary>
    /// Abstract base class for all puzzles.
    /// Provides common solve flow, audio feedback, and event publishing.
    /// Concrete puzzles inherit and implement CheckSolution().
    /// </summary>
    public abstract class PuzzleBase : MonoBehaviour
    {
        [Header("Puzzle Data")]
        [SerializeField] protected PuzzleDataSO data;

        [Header("Audio")]
        [SerializeField] protected AudioSource audioSource;

        [Header("Feedback")]
        [SerializeField] protected GameObject solvedVisualFeedback;

        public bool IsSolved { get; protected set; }
        public string PuzzleId => data != null ? data.puzzleId : "";

        protected virtual void Awake()
        {
            if (solvedVisualFeedback != null)
            {
                solvedVisualFeedback.SetActive(false);
            }
        }

        protected virtual void Start()
        {
            // Check if already solved (e.g., state restored)
            if (GameManager.Instance != null && GameManager.Instance.IsPuzzleSolved(PuzzleId))
            {
                IsSolved = true;
                OnAlreadySolved();
            }
        }

        /// <summary>
        /// Each puzzle implements its own solution-checking logic.
        /// Call CompletePuzzle() when the solution is correct.
        /// </summary>
        public abstract void CheckSolution();

        /// <summary>
        /// Called when the player submits a wrong answer.
        /// </summary>
        protected virtual void OnFailedAttempt()
        {
            EventBus.PuzzleAttemptFailed(PuzzleId);
            PlaySound(data?.failSound);
            Debug.Log($"[Puzzle] {data?.displayName}: Wrong answer!");
        }

        /// <summary>
        /// Marks puzzle as solved, stores digit, plays feedback.
        /// </summary>
        protected void CompletePuzzle()
        {
            if (IsSolved) return;

            IsSolved = true;

            // Update game state
            GameManager.Instance.SolvePuzzle(data.puzzleId);

            // Store revealed digit
            if (!string.IsNullOrEmpty(data.rewardDigitKey))
            {
                GameManager.Instance.RevealDigit(data.rewardDigitKey, data.rewardDigitValue);
            }

            // Publish event
            EventBus.PuzzleSolved(data.puzzleId);

            // Audio feedback
            PlaySound(data?.solveSound);

            // Visual feedback
            if (solvedVisualFeedback != null)
            {
                solvedVisualFeedback.SetActive(true);
            }

            Debug.Log($"[Puzzle] {data?.displayName}: SOLVED! Digit '{data.rewardDigitKey}' = {data.rewardDigitValue}");

            OnSolvedVisual();
        }

        /// <summary>
        /// Override for custom solved animations/effects.
        /// </summary>
        protected virtual void OnSolvedVisual() { }

        /// <summary>
        /// Called when puzzle was already solved (e.g., loading saved state).
        /// Override to set the correct visual state without animation.
        /// </summary>
        protected virtual void OnAlreadySolved()
        {
            if (solvedVisualFeedback != null)
            {
                solvedVisualFeedback.SetActive(true);
            }
        }

        protected void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
