using UnityEngine;
using System.Collections;
using PuzzleGame.Transition;

namespace PuzzleGame.UI
{
    /// <summary>
    /// Victory/end screen controller. Fades in after game completion.
    /// </summary>
    public class EndScreenController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private FadeController fadeController;

        [Header("Settings")]
        [SerializeField] private float delayBeforeShow = 2.0f;
        [SerializeField] private float fadeInDuration = 1.5f;

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
            gameObject.SetActive(true);
            StartCoroutine(EndSequence());
        }

        private IEnumerator EndSequence()
        {
            // Wait for door animation
            yield return new WaitForSeconds(delayBeforeShow);

            // Fade to white
            if (fadeController != null)
            {
                yield return fadeController.FadeToWhite(fadeInDuration);
            }

            // Show end screen
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
