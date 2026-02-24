using UnityEngine;

namespace PuzzleGame.Puzzles
{
    /// <summary>
    /// ScriptableObject containing puzzle configuration data.
    /// Create instances via Assets > Create > PuzzleGame > Puzzle Data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPuzzleData", menuName = "PuzzleGame/Puzzle Data")]
    public class PuzzleDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique puzzle identifier (e.g., 'dial_safe', 'switch_puzzle').")]
        public string puzzleId;

        [Tooltip("Display name for debug logs.")]
        public string displayName;

        [Header("Solution")]
        [Tooltip("The solution values (interpretation depends on puzzle type).")]
        public int[] solution;

        [Header("Reward")]
        [Tooltip("The key for the digit this puzzle reveals (e.g., 'digit_0').")]
        public string rewardDigitKey;

        [Tooltip("The digit value this puzzle reveals.")]
        public int rewardDigitValue;

        [Header("Dependencies")]
        [Tooltip("Puzzle IDs that must be solved before this puzzle becomes active.")]
        public string[] requiredPuzzleIds;

        [Header("Audio")]
        [Tooltip("Sound to play on successful solve.")]
        public AudioClip solveSound;

        [Tooltip("Sound to play on failed attempt.")]
        public AudioClip failSound;
    }
}
