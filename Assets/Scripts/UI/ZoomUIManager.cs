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

        private void Start()
        {
            if (zoomPanel != null)
            {
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
                zoomImage.sprite = sprite;
                currentLockedMessage = lockedMessage; // Store the message meant for the *zoomed* state
                zoomPanel.SetActive(true);
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
                zoomPanel.SetActive(false);
                currentLockedMessage = ""; // Clear on close
                Debug.Log("Zoom closed.");
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
