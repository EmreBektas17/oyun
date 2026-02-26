using UnityEngine;
using UnityEngine.UI;
using PuzzleGame.UI;

namespace PuzzleGame.Interaction
{
    [RequireComponent(typeof(Button))]
    public class Hotspot : MonoBehaviour
    {
        [Header("Hotspot Settings")]
        public string hotspotId;
        
        [Tooltip("The sprite to display when this hotspot is clicked and zoomed in.")]
        public Sprite zoomSprite;

        [Header("Lock Settings")]
        [Tooltip("If true, clicking will not open zoom but instead display a locked message.")]
        public bool showLockedMessage = false;
        public string lockedMessage = "It's locked.";

        private Button button;

        protected virtual void Awake()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }
        }

        protected virtual void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
            }
        }

        public virtual void OnClick()
        {
            if (showLockedMessage)
            {
                // If it's locked but we have a zoom sprite (e.g. view the door closer),
                // we open the zoom view and pass the locked message so clicking the zoomed view shows the text.
                if (ZoomUIManager.Instance != null && zoomSprite != null)
                {
                    ZoomUIManager.Instance.OpenZoom(hotspotId, zoomSprite, lockedMessage);
                }
                // If it's locked but no zoom sprite exists (e.g. just a text popup on the main room)
                else if (ZoomUIManager.Instance != null)
                {
                    ZoomUIManager.Instance.ShowMessage(lockedMessage);
                    Debug.Log($"Showing localized locked message: {lockedMessage}");
                }
                else
                {
                    Debug.LogWarning($"Locked Message (No UI): {lockedMessage}");
                }
                return;
            }

            // Normal unlocked behavior
            if (ZoomUIManager.Instance != null && zoomSprite != null)
            {
                ZoomUIManager.Instance.OpenZoom(hotspotId, zoomSprite);
            }
            else
            {
                Debug.LogWarning($"Missing ZoomUIManager Instance or Zoom Sprite on hotspot: {hotspotId}");
            }
        }
    }
}
