using UnityEngine;
using System.Collections;

namespace PuzzleGame.UI
{
    /// <summary>
    /// Animated "puzzle solved" feedback overlay.
    /// Shows a checkmark/glow that fades in, holds, and fades out.
    /// </summary>
    public class SolvedFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Transform iconTransform;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float holdDuration = 1.0f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float scaleFrom = 0.5f;
        [SerializeField] private float scaleTo = 1.0f;

        private Coroutine animCoroutine;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            // Start hidden
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        public void Show()
        {
            if (animCoroutine != null)
            {
                StopCoroutine(animCoroutine);
            }

            gameObject.SetActive(true);
            animCoroutine = StartCoroutine(PlayFeedback());
        }

        private IEnumerator PlayFeedback()
        {
            // Reset
            canvasGroup.alpha = 0f;
            if (iconTransform != null)
            {
                iconTransform.localScale = Vector3.one * scaleFrom;
            }

            // Fade in + scale up
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeInDuration);
                canvasGroup.alpha = t;

                if (iconTransform != null)
                {
                    float scale = Mathf.Lerp(scaleFrom, scaleTo, t);
                    iconTransform.localScale = Vector3.one * scale;
                }

                yield return null;
            }

            canvasGroup.alpha = 1f;
            if (iconTransform != null)
            {
                iconTransform.localScale = Vector3.one * scaleTo;
            }

            // Hold
            yield return new WaitForSeconds(holdDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeOutDuration);
                canvasGroup.alpha = 1f - t;
                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            animCoroutine = null;
        }
    }
}
