using UnityEngine;
using System.Collections;

namespace PuzzleGame.Transition
{
    /// <summary>
    /// Controls an individual zoom view panel.
    /// Supports both CanvasGroup-based and pure GameObject-based views.
    /// </summary>
    public class ZoomViewController : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string viewId;

        [Header("References (Optional)")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Settings")]
        [SerializeField] private float animationDuration = 0.25f;

        public string ViewId => viewId;

        private Coroutine animCoroutine;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            SetHiddenImmediate();
        }

        public void Show()
        {
            gameObject.SetActive(true);

            if (animCoroutine != null)
            {
                StopCoroutine(animCoroutine);
            }

            if (canvasGroup != null)
            {
                animCoroutine = StartCoroutine(AnimateAlpha(1f));
            }
        }

        public void Hide()
        {
            if (animCoroutine != null)
            {
                StopCoroutine(animCoroutine);
            }

            if (canvasGroup != null)
            {
                animCoroutine = StartCoroutine(AnimateAlphaAndDeactivate(0f));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void SetHiddenImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
        }

        private IEnumerator AnimateAlpha(float target)
        {
            float start = canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);
                canvasGroup.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }

            canvasGroup.alpha = target;
            canvasGroup.interactable = target > 0.5f;
            canvasGroup.blocksRaycasts = target > 0.5f;
            animCoroutine = null;
        }

        private IEnumerator AnimateAlphaAndDeactivate(float target)
        {
            yield return AnimateAlpha(target);
            gameObject.SetActive(false);
        }
    }
}
