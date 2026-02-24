using UnityEngine;
using PuzzleGame.Puzzles;

namespace PuzzleGame.Interaction
{
    /// <summary>
    /// Generic puzzle element click handler.
    /// Attach to clickable puzzle parts (dial rings, switches, symbols, keypad buttons).
    /// Interactable.OnClicked -> Execute().
    /// </summary>
    public class PuzzleElementClick : MonoBehaviour
    {
        public enum ClickAction
        {
            DialRotate,
            SwitchToggle,
            SymbolPress,
            KeypadDigit,
            KeypadDelete,
            PuzzleCheck,
            BookClick
        }

        [Header("Configuration")]
        public ClickAction action;
        public int parameterValue;
        public PuzzleBase targetPuzzle;

        public void Execute()
        {
            if (targetPuzzle == null) return;

            switch (action)
            {
                case ClickAction.DialRotate:
                    (targetPuzzle as DialPuzzle)?.RotateRing(parameterValue);
                    break;
                case ClickAction.SwitchToggle:
                    (targetPuzzle as SwitchPuzzle)?.ToggleSwitch(parameterValue);
                    break;
                case ClickAction.SymbolPress:
                    (targetPuzzle as SymbolPuzzle)?.PressSymbol(parameterValue);
                    break;
                case ClickAction.KeypadDigit:
                    (targetPuzzle as CodePanelPuzzle)?.EnterDigit(parameterValue);
                    break;
                case ClickAction.KeypadDelete:
                    (targetPuzzle as CodePanelPuzzle)?.DeleteDigit();
                    break;
                case ClickAction.PuzzleCheck:
                    targetPuzzle.CheckSolution();
                    break;
                case ClickAction.BookClick:
                    (targetPuzzle as BookCluePuzzle)?.OnBookClicked();
                    break;
            }
        }
    }
}
