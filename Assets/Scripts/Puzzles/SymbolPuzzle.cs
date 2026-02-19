using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PuzzleGame.Puzzles
{
    /// <summary>
    /// Symbol sequence puzzle. Player must click 4 symbols in the correct order.
    /// Wrong symbol resets the sequence. solution[] holds symbol indices in order.
    /// </summary>
    public class SymbolPuzzle : PuzzleBase
    {
        [Header("Symbols")]
        [SerializeField] private SpriteRenderer[] symbolButtons;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = new Color(0.7f, 0.7f, 0.7f);
        [SerializeField] private Color activeColor = new Color(0.3f, 0.9f, 0.5f);
        [SerializeField] private Color wrongColor = new Color(0.9f, 0.3f, 0.3f);

        [Header("Sequence Indicator")]
        [SerializeField] private SpriteRenderer[] sequenceDots;
        [SerializeField] private Color dotActiveColor = Color.white;
        [SerializeField] private Color dotInactiveColor = new Color(0.3f, 0.3f, 0.3f);

        private List<int> currentSequence = new List<int>();
        private bool isResetting;

        protected override void Awake()
        {
            base.Awake();
            ResetVisuals();
        }

        /// <summary>
        /// Called when a symbol button is pressed. Pass the button index.
        /// Wire from Interactable.OnClicked in Inspector.
        /// </summary>
        public void PressSymbol(int symbolIndex)
        {
            if (IsSolved || isResetting) return;
            if (symbolIndex < 0 || symbolIndex >= symbolButtons.Length) return;

            int expectedIndex = data.solution[currentSequence.Count];

            if (symbolIndex == expectedIndex)
            {
                // Correct — add to sequence
                currentSequence.Add(symbolIndex);
                symbolButtons[symbolIndex].color = activeColor;

                // Update sequence indicator
                UpdateSequenceDots();

                // Scale punch feedback
                StartCoroutine(ScalePunch(symbolButtons[symbolIndex].transform));

                // Check if complete
                if (currentSequence.Count >= data.solution.Length)
                {
                    CheckSolution();
                }
            }
            else
            {
                // Wrong — flash and reset
                StartCoroutine(WrongSequence(symbolIndex));
            }
        }

        public override void CheckSolution()
        {
            if (IsSolved) return;

            if (currentSequence.Count == data.solution.Length)
            {
                CompletePuzzle();
            }
        }

        private IEnumerator WrongSequence(int wrongIndex)
        {
            isResetting = true;

            // Flash wrong color
            symbolButtons[wrongIndex].color = wrongColor;
            OnFailedAttempt();

            yield return new WaitForSeconds(0.5f);

            // Reset
            currentSequence.Clear();
            ResetVisuals();

            isResetting = false;
        }

        private void ResetVisuals()
        {
            foreach (var sr in symbolButtons)
            {
                if (sr != null) sr.color = normalColor;
            }

            UpdateSequenceDots();
        }

        private void UpdateSequenceDots()
        {
            if (sequenceDots == null) return;

            for (int i = 0; i < sequenceDots.Length; i++)
            {
                if (sequenceDots[i] != null)
                {
                    sequenceDots[i].color = i < currentSequence.Count ? dotActiveColor : dotInactiveColor;
                }
            }
        }

        private IEnumerator ScalePunch(Transform t)
        {
            Vector3 original = t.localScale;
            Vector3 punched = original * 1.15f;

            float elapsed = 0f;
            float duration = 0.15f;

            // Scale up
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(original, punched, elapsed / (duration / 2f));
                yield return null;
            }

            // Scale back
            elapsed = 0f;
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(punched, original, elapsed / (duration / 2f));
                yield return null;
            }

            t.localScale = original;
        }

        protected override void OnAlreadySolved()
        {
            base.OnAlreadySolved();
            // Show all symbols as active
            foreach (var sr in symbolButtons)
            {
                if (sr != null) sr.color = activeColor;
            }

            if (sequenceDots != null)
            {
                foreach (var dot in sequenceDots)
                {
                    if (dot != null) dot.color = dotActiveColor;
                }
            }
        }
    }
}
