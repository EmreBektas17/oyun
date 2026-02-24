using System;

namespace PuzzleGame.Core
{
    /// <summary>
    /// Static event hub for decoupled communication between game systems.
    /// All game-wide events are published and subscribed through here.
    /// </summary>
    public static class EventBus
    {
        // Puzzle Events
        public static event Action<string> OnPuzzleSolved;
        public static event Action<string> OnPuzzleAttemptFailed;

        // Zoom/Transition Events
        public static event Action<string> OnZoomEnter;
        public static event Action OnZoomExit;

        // Interaction Events
        public static event Action<string> OnInteract;
        public static event Action<string> OnHoverEnter;
        public static event Action<string> OnHoverExit;

        // Game Flow Events
        public static event Action OnGameComplete;
        public static event Action OnGameStart;

        // --- Publishers ---

        public static void PuzzleSolved(string puzzleId)
        {
            OnPuzzleSolved?.Invoke(puzzleId);
        }

        public static void PuzzleAttemptFailed(string puzzleId)
        {
            OnPuzzleAttemptFailed?.Invoke(puzzleId);
        }

        public static void ZoomEnter(string viewId)
        {
            OnZoomEnter?.Invoke(viewId);
        }

        public static void ZoomExit()
        {
            OnZoomExit?.Invoke();
        }

        public static void Interact(string interactableId)
        {
            OnInteract?.Invoke(interactableId);
        }

        public static void HoverEnter(string interactableId)
        {
            OnHoverEnter?.Invoke(interactableId);
        }

        public static void HoverExit(string interactableId)
        {
            OnHoverExit?.Invoke(interactableId);
        }

        public static void GameComplete()
        {
            OnGameComplete?.Invoke();
        }

        public static void GameStart()
        {
            OnGameStart?.Invoke();
        }

        /// <summary>
        /// Clears all event subscriptions. Call on scene unload to prevent leaks.
        /// </summary>
        public static void ClearAll()
        {
            OnPuzzleSolved = null;
            OnPuzzleAttemptFailed = null;
            OnZoomEnter = null;
            OnZoomExit = null;
            OnInteract = null;
            OnHoverEnter = null;
            OnHoverExit = null;
            OnGameComplete = null;
            OnGameStart = null;
        }
    }
}
