using UnityEngine;
using System.Collections;

namespace PuzzleGame.Puzzles
{
    /// <summary>
    /// Binary switch puzzle. Player toggles switches on/off.
    /// Auto-checks solution after each toggle.
    /// solution[] = { 1, 0, 1, 1, 0 } (1=on, 0=off)
    /// </summary>
    public class SwitchPuzzle : PuzzleBase
    {
        [Header("Switches")]
        [SerializeField] private Transform[] switchHandles;

        [Header("Visual Settings")]
        [SerializeField] private float switchOnAngle = 30f;
        [SerializeField] private float switchOffAngle = -30f;
        [SerializeField] private float switchSpeed = 10f;

        [Header("Switch Colors")]
        [SerializeField] private Color onColor = new Color(0.2f, 0.8f, 0.3f);
        [SerializeField] private Color offColor = new Color(0.6f, 0.6f, 0.6f);

        private bool[] switchStates;
        private SpriteRenderer[] switchRenderers;

        protected override void Awake()
        {
            base.Awake();
            switchStates = new bool[switchHandles.Length];
            switchRenderers = new SpriteRenderer[switchHandles.Length];

            for (int i = 0; i < switchHandles.Length; i++)
            {
                switchRenderers[i] = switchHandles[i].GetComponent<SpriteRenderer>();
            }
        }

        /// <summary>
        /// Toggle a specific switch. Wire from Interactable.OnClicked.
        /// </summary>
        public void ToggleSwitch(int switchIndex)
        {
            if (IsSolved) return;
            if (switchIndex < 0 || switchIndex >= switchHandles.Length) return;

            switchStates[switchIndex] = !switchStates[switchIndex];

            // Animate the switch
            float targetAngle = switchStates[switchIndex] ? switchOnAngle : switchOffAngle;
            StartCoroutine(AnimateSwitch(switchHandles[switchIndex], targetAngle));

            // Update color
            if (switchRenderers[switchIndex] != null)
            {
                switchRenderers[switchIndex].color = switchStates[switchIndex] ? onColor : offColor;
            }

            // Auto-check after each toggle
            CheckSolution();
        }

        public override void CheckSolution()
        {
            if (IsSolved) return;
            if (data?.solution == null) return;

            bool correct = true;
            for (int i = 0; i < data.solution.Length && i < switchStates.Length; i++)
            {
                bool expected = data.solution[i] == 1;
                if (switchStates[i] != expected)
                {
                    correct = false;
                    break;
                }
            }

            if (correct)
            {
                CompletePuzzle();
            }
        }

        private IEnumerator AnimateSwitch(Transform handle, float targetAngle)
        {
            Quaternion target = Quaternion.Euler(0f, 0f, targetAngle);

            while (Quaternion.Angle(handle.localRotation, target) > 0.5f)
            {
                handle.localRotation = Quaternion.Lerp(
                    handle.localRotation, target, Time.deltaTime * switchSpeed);
                yield return null;
            }

            handle.localRotation = target;
        }

        protected override void OnAlreadySolved()
        {
            base.OnAlreadySolved();
            if (data?.solution == null) return;

            for (int i = 0; i < data.solution.Length && i < switchHandles.Length; i++)
            {
                switchStates[i] = data.solution[i] == 1;
                float angle = switchStates[i] ? switchOnAngle : switchOffAngle;
                switchHandles[i].localRotation = Quaternion.Euler(0f, 0f, angle);

                if (switchRenderers[i] != null)
                {
                    switchRenderers[i].color = switchStates[i] ? onColor : offColor;
                }
            }
        }
    }
}
