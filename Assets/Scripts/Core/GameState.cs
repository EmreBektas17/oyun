using System.Collections.Generic;

namespace PuzzleGame.Core
{
    /// <summary>
    /// Serializable data container for all game progress state.
    /// Kept separate from GameManager for clean separation of data and logic.
    /// </summary>
    [System.Serializable]
    public class GameState
    {
        /// <summary>
        /// Set of puzzle IDs that have been solved.
        /// </summary>
        public HashSet<string> solvedPuzzles = new HashSet<string>();

        /// <summary>
        /// Digits revealed by solving puzzles.
        /// Key: position index (0-3), Value: digit value.
        /// </summary>
        public Dictionary<string, int> revealedDigits = new Dictionary<string, int>();

        /// <summary>
        /// Whether the game has been completed (vault opened).
        /// </summary>
        public bool gameComplete;

        /// <summary>
        /// Total number of code digits needed to complete the game.
        /// </summary>
        public const int TOTAL_DIGITS_REQUIRED = 4;

        public bool IsPuzzleSolved(string puzzleId)
        {
            return solvedPuzzles.Contains(puzzleId);
        }

        public void MarkPuzzleSolved(string puzzleId)
        {
            solvedPuzzles.Add(puzzleId);
        }

        public void AddRevealedDigit(string key, int value)
        {
            revealedDigits[key] = value;
        }

        public bool AreAllDigitsFound()
        {
            return revealedDigits.Count >= TOTAL_DIGITS_REQUIRED;
        }

        /// <summary>
        /// Returns the code digits in order (digit_0, digit_1, digit_2, digit_3).
        /// </summary>
        public int[] GetCodeDigits()
        {
            int[] code = new int[TOTAL_DIGITS_REQUIRED];
            for (int i = 0; i < TOTAL_DIGITS_REQUIRED; i++)
            {
                string key = "digit_" + i;
                if (revealedDigits.ContainsKey(key))
                {
                    code[i] = revealedDigits[key];
                }
            }
            return code;
        }

        public void Reset()
        {
            solvedPuzzles.Clear();
            revealedDigits.Clear();
            gameComplete = false;
        }
    }
}
