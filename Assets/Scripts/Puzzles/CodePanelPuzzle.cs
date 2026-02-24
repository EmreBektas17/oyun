using UnityEngine;
using System.Collections;

namespace PuzzleGame.Puzzles
{
    /// <summary>
    /// 4-digit code entry panel using world-space TextMesh components.
    /// Player types digits via keypad buttons. Submit checks against collected code.
    /// </summary>
    public class CodePanelPuzzle : PuzzleBase
    {
        [Header("Display")]
        [Tooltip("TextMesh components for each digit slot.")]
        [SerializeField] private TextMesh[] digitDisplays = new TextMesh[4];

        [Header("Visual")]
        [SerializeField] private Color normalDigitColor = Color.white;
        [SerializeField] private Color correctColor = new Color(0.2f, 0.9f, 0.3f);
        [SerializeField] private Color wrongColor = new Color(0.9f, 0.3f, 0.3f);

        private int[] enteredDigits = new int[4];
        private int currentSlot = 0;

        protected override void Awake()
        {
            base.Awake();
            ClearDisplay();
        }

        /// <summary>
        /// Called when a keypad button (0-9) is pressed.
        /// </summary>
        public void EnterDigit(int digit)
        {
            if (IsSolved) return;
            if (currentSlot >= 4) return;

            enteredDigits[currentSlot] = digit;

            if (digitDisplays[currentSlot] != null)
            {
                digitDisplays[currentSlot].text = digit.ToString();
                digitDisplays[currentSlot].color = normalDigitColor;
            }

            currentSlot++;
        }

        /// <summary>
        /// Remove last entered digit.
        /// </summary>
        public void DeleteDigit()
        {
            if (IsSolved) return;
            if (currentSlot <= 0) return;

            currentSlot--;
            enteredDigits[currentSlot] = 0;

            if (digitDisplays[currentSlot] != null)
            {
                digitDisplays[currentSlot].text = "_";
                digitDisplays[currentSlot].color = normalDigitColor;
            }
        }

        /// <summary>
        /// Submit the entered code for validation.
        /// </summary>
        public override void CheckSolution()
        {
            if (IsSolved) return;
            if (currentSlot < 4) return;

            int[] correctCode = Core.GameManager.Instance.GetCode();

            bool correct = true;
            for (int i = 0; i < 4; i++)
            {
                if (enteredDigits[i] != correctCode[i])
                {
                    correct = false;
                    break;
                }
            }

            if (correct)
            {
                StartCoroutine(FlashDigits(correctColor, false));
                CompletePuzzle();
            }
            else
            {
                StartCoroutine(FlashDigits(wrongColor, true));
                OnFailedAttempt();
            }
        }

        public void ClearDisplay()
        {
            currentSlot = 0;
            enteredDigits = new int[4];

            for (int i = 0; i < digitDisplays.Length; i++)
            {
                if (digitDisplays[i] != null)
                {
                    digitDisplays[i].text = "_";
                    digitDisplays[i].color = normalDigitColor;
                }
            }
        }

        private IEnumerator FlashDigits(Color color, bool resetAfter)
        {
            foreach (var display in digitDisplays)
            {
                if (display != null) display.color = color;
            }

            if (resetAfter)
            {
                yield return ShakeFeedback();
                yield return new WaitForSeconds(0.3f);
                ClearDisplay();
            }
        }

        private IEnumerator ShakeFeedback()
        {
            Transform t = transform;
            Vector3 original = t.localPosition;
            float elapsed = 0f;
            float duration = 0.4f;
            float magnitude = 0.08f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Mathf.Sin(elapsed * 40f) * magnitude * (1f - elapsed / duration);
                t.localPosition = original + new Vector3(x, 0f, 0f);
                yield return null;
            }

            t.localPosition = original;
        }

        protected override void OnAlreadySolved()
        {
            base.OnAlreadySolved();
            int[] code = Core.GameManager.Instance.GetCode();
            for (int i = 0; i < 4 && i < digitDisplays.Length; i++)
            {
                if (digitDisplays[i] != null)
                {
                    digitDisplays[i].text = code[i].ToString();
                    digitDisplays[i].color = correctColor;
                }
            }
        }
    }
}
