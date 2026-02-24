using UnityEngine;
using PuzzleGame.Core;

namespace PuzzleGame.UI
{
    /// <summary>
    /// Central UI manager. Subscribes to game events and triggers
    /// appropriate UI feedback (solved overlay, end screen).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private SolvedFeedback solvedFeedback;
        [SerializeField] private EndScreenController endScreen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnPuzzleSolved += HandlePuzzleSolved;
            EventBus.OnPuzzleAttemptFailed += HandlePuzzleFailed;
            EventBus.OnGameComplete += HandleGameComplete;
        }

        private void OnDisable()
        {
            EventBus.OnPuzzleSolved -= HandlePuzzleSolved;
            EventBus.OnPuzzleAttemptFailed -= HandlePuzzleFailed;
            EventBus.OnGameComplete -= HandleGameComplete;
        }

        private void HandlePuzzleSolved(string puzzleId)
        {
            if (solvedFeedback != null)
            {
                solvedFeedback.Show();
            }

            Debug.Log($"[UIManager] Puzzle solved UI feedback: {puzzleId}");
        }

        private void HandlePuzzleFailed(string puzzleId)
        {
            Debug.Log($"[UIManager] Puzzle failed UI feedback: {puzzleId}");
        }

        private void HandleGameComplete()
        {
            if (endScreen != null)
            {
                endScreen.Show();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
