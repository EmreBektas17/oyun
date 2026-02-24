using UnityEngine;
using UnityEngine.InputSystem;

namespace PuzzleGame.Interaction
{
    /// <summary>
    /// Attached to the main camera. Performs 2D point overlap checks each frame
    /// to detect Interactable objects under the cursor and delegates
    /// hover/click events to them.
    /// Uses the new Input System package.
    /// </summary>
    public class InteractionRaycaster : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private LayerMask interactableLayer = ~0;
        [SerializeField] private bool isActive = true;

        private Camera mainCamera;
        private Interactable currentHovered;

        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        private void Awake()
        {
            mainCamera = GetComponent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (!isActive || mainCamera == null) return;
            if (Mouse.current == null) return;

            DetectHover();
            HandleClick();
        }

        private void DetectHover()
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

            Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos, interactableLayer);

            if (hitCollider != null)
            {
                Interactable interactable = hitCollider.GetComponent<Interactable>();

                if (interactable != null)
                {
                    if (currentHovered != interactable)
                    {
                        currentHovered?.HoverExit();
                        currentHovered = interactable;
                        currentHovered.HoverEnter();
                    }
                }
                else
                {
                    ClearHover();
                }
            }
            else
            {
                ClearHover();
            }
        }

        private void HandleClick()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && currentHovered != null)
            {
                currentHovered.Interact();
            }
        }

        private void ClearHover()
        {
            if (currentHovered != null)
            {
                currentHovered.HoverExit();
                currentHovered = null;
            }
        }

        public void SetActive(bool active)
        {
            isActive = active;
            if (!active)
            {
                ClearHover();
            }
        }
    }
}
