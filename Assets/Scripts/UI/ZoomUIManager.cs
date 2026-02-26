using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Required for Coroutines
using TMPro; // Added for TextMeshPro support

namespace PuzzleGame.UI
{
    public class ZoomUIManager : MonoBehaviour
    {
        public static ZoomUIManager Instance { get; private set; }

        [Header("Zoom Panel Elements")]
        [Tooltip("The full screen panel for the zoom view")]
        public GameObject zoomPanel;
        [Tooltip("The Image component that displays the zoomed sprite")]
        public Image zoomImage;

        [Header("Message System")]
        [Tooltip("TextMeshPro component to display locked/hint messages")]
        public TextMeshProUGUI messageText;
        [Tooltip("How long the message stays on screen")]
        public float messageDuration = 2f;

        private Coroutine messageCoroutine;

        private string currentLockedMessage = "";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            // Try to assign button logic dynamically to the ZoomImage if not setup in Editor
            if (zoomImage != null)
            {
                // Crucial: Make sure the image can actually receive clicks
                zoomImage.raycastTarget = true;

                Button imgBtn = zoomImage.GetComponent<Button>();
                if (imgBtn == null)
                {
                    imgBtn = zoomImage.gameObject.AddComponent<Button>();
                    ColorBlock cb = imgBtn.colors;
                    cb.normalColor = Color.white;
                    cb.highlightedColor = Color.white;
                    cb.pressedColor = Color.white;
                    cb.selectedColor = Color.white;
                    cb.disabledColor = Color.white;
                    imgBtn.colors = cb;
                }
                // Clear any old listeners to prevent duplicates if Awake runs multiple times or scenes reload
                imgBtn.onClick.RemoveListener(OnZoomImageClicked);
                imgBtn.onClick.AddListener(OnZoomImageClicked);
            }
        }

        private void OnZoomImageClicked()
        {
            // If there's a locked message assigned for this zoomed view, display it when clicking the image
            if (!string.IsNullOrEmpty(currentLockedMessage))
            {
                ShowMessage(currentLockedMessage);
            }
        }

        [Header("Animation Settings")]
        [Tooltip("How long the zoom animation takes (in seconds)")]
        public float zoomAnimationDuration = 0.25f;

        private Coroutine zoomCoroutine;

        private void Start()
        {
            if (zoomPanel != null)
            {
                // Ensure fully transparent and inactive initially
                Image panelBg = zoomPanel.GetComponent<Image>();
                if (panelBg != null)
                {
                    Color c = panelBg.color;
                    panelBg.color = new Color(c.r, c.g, c.b, 0f);
                }
                zoomPanel.SetActive(false);
            }

            if (messageText != null)
            {
                messageText.gameObject.SetActive(false);
            }
        }

        public void OpenZoom(string hotspotId, Sprite sprite, string lockedMessage = "")
        {
            if (zoomPanel == null || zoomImage == null)
            {
                Debug.LogWarning("Zoom Panel or Zoom Image reference is missing in ZoomUIManager!");
                return;
            }

            if (sprite != null)
            {
                // Ensure the GameObject this script is attached to is active, otherwise Coroutine won't run.
                gameObject.SetActive(true);
                zoomPanel.SetActive(true); // Must be active to run coroutines if attached here

                zoomImage.sprite = sprite;
                currentLockedMessage = lockedMessage;
                
                if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
                zoomCoroutine = StartCoroutine(AnimateZoom(true));
                
                Debug.Log($"Zoom opened for hotspot: {hotspotId}");
            }
            else
            {
                Debug.LogWarning($"Trying to open zoom for {hotspotId} but sprite is null!");
            }
        }

        public void CloseZoom()
        {
            if (zoomPanel != null)
            {
                currentLockedMessage = "";
                
                if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
                zoomCoroutine = StartCoroutine(AnimateZoom(false));

                Debug.Log("Zoom closed.");
            }
        }

        private IEnumerator AnimateZoom(bool isOpening)
        {
            float elapsedTime = 0f;
            Image panelBg = zoomPanel.GetComponent<Image>();
            
            // Starting states
            if (isOpening)
            {
                zoomPanel.SetActive(true);
                zoomImage.transform.localScale = Vector3.one * 0.5f; // Start small
                
                // Start panel transparent
                if (panelBg != null)
                {
                    Color c = panelBg.color;
                    panelBg.color = new Color(c.r, c.g, c.b, 0f);
                }
                
                // Start image transparent
                Color imgC = zoomImage.color;
                zoomImage.color = new Color(imgC.r, imgC.g, imgC.b, 0f);
            }

            Vector3 startScale = isOpening ? Vector3.one * 0.5f : Vector3.one;
            Vector3 targetScale = isOpening ? Vector3.one : Vector3.one * 0.5f;
            float startAlpha = isOpening ? 0f : 0.9f; 
            float targetAlpha = isOpening ? 0.9f : 0f;
            
            float startAlphaImg = isOpening ? 0f : 1f;
            float targetAlphaImg = isOpening ? 1f : 0f;

            while (elapsedTime < zoomAnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / zoomAnimationDuration);
                
                // Smooth Step for nicer easing
                t = t * t * (3f - 2f * t);

                // Animate Scale
                zoomImage.transform.localScale = Vector3.Lerp(startScale, targetScale, t);

                // Animate Panel Background Alpha
                if (panelBg != null)
                {
                    Color c = panelBg.color;
                    panelBg.color = new Color(c.r, c.g, c.b, Mathf.Lerp(startAlpha, targetAlpha, t));
                }

                // Animate Image Alpha
                Color imgColor = zoomImage.color;
                zoomImage.color = new Color(imgColor.r, imgColor.g, imgColor.b, Mathf.Lerp(startAlphaImg, targetAlphaImg, t));

                yield return null;
            }

            // Guarantee final state
            zoomImage.transform.localScale = targetScale;
            if (panelBg != null)
            {
                Color c = panelBg.color;
                panelBg.color = new Color(c.r, c.g, c.b, targetAlpha);
            }
            
            Color finalImgColor = zoomImage.color;
            zoomImage.color = new Color(finalImgColor.r, finalImgColor.g, finalImgColor.b, targetAlphaImg);

            // Hide if closing
            if (!isOpening)
            {
                zoomPanel.SetActive(false);
            }
        }

        public void ShowMessage(string message)
        {
            if (messageText == null)
            {
                Debug.LogWarning($"Message UI is missing! Wanted to show: {message}");
                return;
            }

            if (messageCoroutine != null)
            {
                StopCoroutine(messageCoroutine);
            }
            
            messageCoroutine = StartCoroutine(MessageRoutine(message));
        }

        private IEnumerator MessageRoutine(string message)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(messageDuration);
            
            messageText.gameObject.SetActive(false);
        }
    }
}
