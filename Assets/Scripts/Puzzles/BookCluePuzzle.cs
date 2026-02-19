using UnityEngine;
using System.Collections;

namespace PuzzleGame.Puzzles
{
    /// <summary>
    /// Simple book clue interaction in the main room.
    /// Player clicks a book → it glows → reveals a hidden digit on the wall.
    /// This is a one-click puzzle with visual reveal animation.
    /// </summary>
    public class BookCluePuzzle : PuzzleBase
    {
        [Header("Book References")]
        [SerializeField] private SpriteRenderer bookRenderer;
        [SerializeField] private GameObject hiddenDigitObject;
        [SerializeField] private SpriteRenderer wallDigitRenderer;

        [Header("Animation Settings")]
        [SerializeField] private Color bookGlowColor = new Color(1f, 0.95f, 0.7f);
        [SerializeField] private float glowDuration = 0.5f;
        [SerializeField] private float revealDelay = 0.3f;

        protected override void Awake()
        {
            base.Awake();

            if (hiddenDigitObject != null)
            {
                hiddenDigitObject.SetActive(false);
            }
        }

        /// <summary>
        /// Called when the book is clicked.
        /// Wire from Interactable.OnClicked on the book hotspot.
        /// </summary>
        public void OnBookClicked()
        {
            if (IsSolved) return;

            StartCoroutine(RevealSequence());
        }

        public override void CheckSolution()
        {
            // Book clue is always "correct" — it just reveals on click
            CompletePuzzle();
        }

        private IEnumerator RevealSequence()
        {
            // Glow the book
            if (bookRenderer != null)
            {
                Color originalColor = bookRenderer.color;
                float elapsed = 0f;

                while (elapsed < glowDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.PingPong(elapsed * 4f, 1f);
                    bookRenderer.color = Color.Lerp(originalColor, bookGlowColor, t);
                    yield return null;
                }

                bookRenderer.color = bookGlowColor;
            }

            yield return new WaitForSeconds(revealDelay);

            // Reveal hidden digit
            if (hiddenDigitObject != null)
            {
                hiddenDigitObject.SetActive(true);

                // Fade in the digit
                if (wallDigitRenderer != null)
                {
                    Color color = wallDigitRenderer.color;
                    wallDigitRenderer.color = new Color(color.r, color.g, color.b, 0f);

                    float elapsed = 0f;
                    while (elapsed < 0.5f)
                    {
                        elapsed += Time.deltaTime;
                        float alpha = Mathf.Lerp(0f, 1f, elapsed / 0.5f);
                        wallDigitRenderer.color = new Color(color.r, color.g, color.b, alpha);
                        yield return null;
                    }

                    wallDigitRenderer.color = new Color(color.r, color.g, color.b, 1f);
                }
            }

            // Complete the puzzle
            CheckSolution();
        }

        protected override void OnAlreadySolved()
        {
            base.OnAlreadySolved();

            if (bookRenderer != null)
            {
                bookRenderer.color = bookGlowColor;
            }

            if (hiddenDigitObject != null)
            {
                hiddenDigitObject.SetActive(true);
            }
        }
    }
}
