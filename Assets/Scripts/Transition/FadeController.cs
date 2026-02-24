using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PuzzleGame.Transition
{
    /// <summary>
    /// Controls the full-screen fade overlay for smooth transitions.
    /// Uses a UI Image that covers the entire screen.
    /// </summary>
    public class FadeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image fadeImage;

        [Header("Settings")]
        [SerializeField] private float defaultFadeDuration = 0.3f;
        [SerializeField] private Color fadeColor = Color.black;

        private void Awake()
        {
            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
                fadeImage.raycastTarget = false;
            }
        }

        /// <summary>
        /// Fade to black (or specified color).
        /// </summary>
        public IEnumerator FadeOut(float duration = -1f)
        {
            if (duration < 0f) duration = defaultFadeDuration;
            yield return FadeTo(1f, duration);
        }

        /// <summary>
        /// Fade from black back to clear.
        /// </summary>
        public IEnumerator FadeIn(float duration = -1f)
        {
            if (duration < 0f) duration = defaultFadeDuration;
            yield return FadeTo(0f, duration);
        }

        /// <summary>
        /// Fade to white (for end sequence).
        /// </summary>
        public IEnumerator FadeToWhite(float duration = 1.5f)
        {
            fadeColor = Color.white;
            if (fadeImage != null)
            {
                fadeImage.color = new Color(1f, 1f, 1f, 0f);
            }
            yield return FadeTo(1f, duration);
        }

        private IEnumerator FadeTo(float targetAlpha, float duration)
        {
            if (fadeImage == null) yield break;

            fadeImage.raycastTarget = true;
            float startAlpha = fadeImage.color.a;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
                yield return null;
            }

            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);

            // Only block raycasts when fully opaque
            fadeImage.raycastTarget = targetAlpha > 0.5f;
        }

        /// <summary>
        /// Immediately set fade state (no animation).
        /// </summary>
        public void SetImmediate(float alpha)
        {
            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                fadeImage.raycastTarget = alpha > 0.5f;
            }
        }
    }
}
