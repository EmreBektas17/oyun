using UnityEngine;

namespace PuzzleGame.Core
{
    /// <summary>
    /// Central game manager singleton. Owns the GameState and orchestrates
    /// high-level game flow by listening to EventBus events.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        private GameState state;

        public GameState State => state;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            state = new GameState();
        }

        private void OnEnable()
        {
            EventBus.OnPuzzleSolved += HandlePuzzleSolved;
            EventBus.OnGameComplete += HandleGameComplete;
        }

        private void OnDisable()
        {
            EventBus.OnPuzzleSolved -= HandlePuzzleSolved;
            EventBus.OnGameComplete -= HandleGameComplete;
        }

        private void Start()
        {
            EventBus.GameStart();
            Log("Game Started");
        }

        // --- Public API ---

        public bool IsPuzzleSolved(string puzzleId)
        {
            return state.IsPuzzleSolved(puzzleId);
        }

        public void SolvePuzzle(string puzzleId)
        {
            if (state.IsPuzzleSolved(puzzleId))
            {
                Log($"Puzzle '{puzzleId}' already solved, ignoring.");
                return;
            }

            state.MarkPuzzleSolved(puzzleId);
            Log($"Puzzle Solved: {puzzleId}");
        }

        public void RevealDigit(string key, int value)
        {
            state.AddRevealedDigit(key, value);
            Log($"Digit Revealed: {key} = {value} (Total: {state.revealedDigits.Count}/{GameState.TOTAL_DIGITS_REQUIRED})");
        }

        public bool AreAllDigitsFound()
        {
            return state.AreAllDigitsFound();
        }

        public int[] GetCode()
        {
            return state.GetCodeDigits();
        }

        public void CompleteGame()
        {
            if (state.gameComplete) return;

            state.gameComplete = true;
            EventBus.GameComplete();
        }

        // --- Event Handlers ---

        private void HandlePuzzleSolved(string puzzleId)
        {
            Log($"[Event] PuzzleSolved received: {puzzleId}");

            if (state.AreAllDigitsFound())
            {
                Log("All digits found! Code panel is now solvable.");
            }
        }

        private void HandleGameComplete()
        {
            Log("=== GAME COMPLETE ===");
        }

        // --- Utility ---

        private void Log(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[GameManager] {message}");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                EventBus.ClearAll();
                Instance = null;
            }
        }
    }
}
