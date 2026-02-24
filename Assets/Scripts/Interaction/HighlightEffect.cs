using UnityEngine;
using System.Collections;

namespace PuzzleGame.Interaction
{
    /// <summary>
    /// Provides a visual highlight effect when the player hovers over an interactable.
    /// Supports sprite color tint and optional outline sprite approaches.
    /// </summary>
    public class HighlightEffect : MonoBehaviour
    {
        [Header("Highlight Settings")]
        [Tooltip("The SpriteRenderer to apply the highlight effect to.")]
        [SerializeField] private SpriteRenderer targetRenderer;

        [Tooltip("Color tint applied on hover.")]
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 1f, 1f);

        [Tooltip("Normal color when not hovered.")]
        [SerializeField] private Color normalColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        [Tooltip("Speed of the color transition.")]
        [SerializeField] private float transitionSpeed = 8f;

        [Header("Optional Outline")]
        [Tooltip("Optional separate outline sprite that appears on hover.")]
        [SerializeField] private GameObject outlineObject;

        [Header("Scale Punch")]
        [Tooltip("Enable subtle scale increase on hover.")]
        [SerializeField] private bool useScalePunch = true;

        [Tooltip("Scale multiplier on hover.")]
        [SerializeField] private float hoverScale = 1.05f;

        private bool isHighlighted;
        private Vector3 originalScale;
        private Color currentColor;
        private Coroutine transitionCoroutine;

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }

            originalScale = transform.localScale;
            currentColor = normalColor;

            if (targetRenderer != null)
            {
                targetRenderer.color = normalColor;
            }

            if (outlineObject != null)
            {
                outlineObject.SetActive(false);
            }
        }

        public void SetHighlight(bool highlighted)
        {
            if (isHighlighted == highlighted) return;

            isHighlighted = highlighted;

            if (outlineObject != null)
            {
                outlineObject.SetActive(highlighted);
            }

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(TransitionEffect(highlighted));
        }

        private IEnumerator TransitionEffect(bool toHighlighted)
        {
            Color targetColor = toHighlighted ? highlightColor : normalColor;
            Vector3 targetScale = toHighlighted && useScalePunch
                ? originalScale * hoverScale
                : originalScale;

            while (true)
            {
                // Color lerp
                if (targetRenderer != null)
                {
                    currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * transitionSpeed);
                    targetRenderer.color = currentColor;
                }

                // Scale lerp
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);

                // Check if close enough to target
                bool colorDone = targetRenderer == null ||
                    ColorApproxEqual(currentColor, targetColor);
                bool scaleDone = Vector3.Distance(transform.localScale, targetScale) < 0.001f;

                if (colorDone && scaleDone)
                {
                    if (targetRenderer != null) targetRenderer.color = targetColor;
                    transform.localScale = targetScale;
                    break;
                }

                yield return null;
            }

            transitionCoroutine = null;
        }

        private bool ColorApproxEqual(Color a, Color b, float threshold = 0.01f)
        {
            return Mathf.Abs(a.r - b.r) < threshold &&
                   Mathf.Abs(a.g - b.g) < threshold &&
                   Mathf.Abs(a.b - b.b) < threshold &&
                   Mathf.Abs(a.a - b.a) < threshold;
        }
    }
}
