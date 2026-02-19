using UnityEngine;
using System.Collections;

namespace PuzzleGame.Puzzles
{
    /// <summary>
    /// 3-ring dial safe puzzle. Player clicks each ring to rotate it.
    /// When all 3 rings match the solution values, clicking Confirm solves the puzzle.
    /// solution[] = { ring0Value, ring1Value, ring2Value } (e.g., {3, 7, 1})
    /// Each ring cycles through values 0-9.
    /// </summary>
    public class DialPuzzle : PuzzleBase
    {
        [Header("Dial Rings")]
        [Tooltip("The 3 dial ring transforms (will be rotated visually).")]
        [SerializeField] private Transform[] dialRings = new Transform[3];

        [Header("Settings")]
        [SerializeField] private int maxValue = 9;
        [SerializeField] private float rotationPerStep = 36f; // 360/10
        [SerializeField] private float rotationSpeed = 8f;

        private int[] currentValues;
        private bool isRotating;

        protected override void Awake()
        {
            base.Awake();
            currentValues = new int[dialRings.Length];
        }

        /// <summary>
        /// Called when a dial ring is clicked. Pass ring index (0, 1, or 2).
        /// Wire from Button.OnClick or Interactable.OnClicked in the Inspector.
        /// </summary>
        public void RotateRing(int ringIndex)
        {
            if (IsSolved || isRotating) return;
            if (ringIndex < 0 || ringIndex >= dialRings.Length) return;

            currentValues[ringIndex] = (currentValues[ringIndex] + 1) % (maxValue + 1);

            float targetAngle = currentValues[ringIndex] * rotationPerStep;
            StartCoroutine(AnimateRingRotation(dialRings[ringIndex], targetAngle));
        }

        /// <summary>
        /// Called when Confirm button is clicked.
        /// </summary>
        public override void CheckSolution()
        {
            if (IsSolved) return;

            bool correct = true;
            for (int i = 0; i < data.solution.Length && i < currentValues.Length; i++)
            {
                if (currentValues[i] != data.solution[i])
                {
                    correct = false;
                    break;
                }
            }

            if (correct)
            {
                CompletePuzzle();
            }
            else
            {
                OnFailedAttempt();
                StartCoroutine(ShakeFeedback());
            }
        }

        private IEnumerator AnimateRingRotation(Transform ring, float targetZ)
        {
            isRotating = true;
            Quaternion target = Quaternion.Euler(0f, 0f, -targetZ);
            
            while (Quaternion.Angle(ring.localRotation, target) > 0.5f)
            {
                ring.localRotation = Quaternion.Lerp(ring.localRotation, target, Time.deltaTime * rotationSpeed);
                yield return null;
            }

            ring.localRotation = target;
            isRotating = false;
        }

        private IEnumerator ShakeFeedback()
        {
            Transform t = transform;
            Vector3 original = t.localPosition;
            float elapsed = 0f;
            float duration = 0.4f;
            float magnitude = 10f;

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
            // Set rings to solved position
            if (data?.solution != null)
            {
                for (int i = 0; i < data.solution.Length && i < dialRings.Length; i++)
                {
                    currentValues[i] = data.solution[i];
                    float angle = data.solution[i] * rotationPerStep;
                    dialRings[i].localRotation = Quaternion.Euler(0f, 0f, -angle);
                }
            }
        }
    }
}
