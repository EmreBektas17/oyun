using UnityEngine;

namespace PuzzleGame.Interaction
{
    /// <summary>
    /// Manages cursor texture changes when hovering over interactable objects.
    /// Singleton - attach to a persistent manager GameObject.
    /// </summary>
    public class CursorManager : MonoBehaviour
    {
        public static CursorManager Instance { get; private set; }

        [Header("Cursor Textures")]
        [Tooltip("Default arrow cursor texture. Leave null for system default.")]
        [SerializeField] private Texture2D defaultCursorTexture;

        [Tooltip("Pointer/hand cursor shown when hovering interactable objects.")]
        [SerializeField] private Texture2D interactCursorTexture;

        [Header("Hotspot")]
        [Tooltip("Pixel offset for cursor hotspot (typically center or tip).")]
        [SerializeField] private Vector2 cursorHotspot = Vector2.zero;

        private bool isInteractCursorActive;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SetDefaultCursor();
        }

        public void SetInteractCursor()
        {
            if (isInteractCursorActive) return;

            isInteractCursorActive = true;
            Cursor.SetCursor(interactCursorTexture, cursorHotspot, CursorMode.Auto);
        }

        public void SetDefaultCursor()
        {
            isInteractCursorActive = false;
            Cursor.SetCursor(defaultCursorTexture, cursorHotspot, CursorMode.Auto);
        }

        private void OnDestroy()
        {
            // Reset to system cursor
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
