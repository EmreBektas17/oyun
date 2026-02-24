using UnityEngine;
using UnityEngine.Events;
using PuzzleGame.Core;

namespace PuzzleGame.Interaction
{
    /// <summary>
    /// Reusable component for any clickable object in the game.
    /// Attach to a GameObject with a Collider2D. Configure in Inspector.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Interactable : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this interactable.")]
        [SerializeField] private string interactableId;

        [Header("Gating")]
        [Tooltip("If true, this interactable requires a specific puzzle to be solved first.")]
        [SerializeField] private bool requiresPuzzleSolved;
        [Tooltip("The puzzle ID that must be solved before this interactable works.")]
        [SerializeField] private string requiredPuzzleId;

        [Header("Behavior")]
        [Tooltip("If true, this interactable can only be clicked once.")]
        [SerializeField] private bool oneTimeUse;

        [Header("Events")]
        [Tooltip("Fired when this interactable is successfully clicked.")]
        public UnityEvent OnClicked;

        [Header("Visual")]
        [Tooltip("Reference to the HighlightEffect component (optional).")]
        [SerializeField] private HighlightEffect highlightEffect;

        public string Id => interactableId;

        private bool hasBeenUsed;
        private bool isHovered;

        private void Awake()
        {
            if (highlightEffect == null)
            {
                highlightEffect = GetComponent<HighlightEffect>();
            }
        }

        private void OnEnable()
        {
            EventBus.OnPuzzleSolved += HandlePuzzleSolved;
        }

        private void OnDisable()
        {
            EventBus.OnPuzzleSolved -= HandlePuzzleSolved;
        }

        /// <summary>
        /// Called by InteractionRaycaster when this object is clicked.
        /// </summary>
        public void Interact()
        {
            if (!CanInteract()) return;

            if (oneTimeUse)
            {
                hasBeenUsed = true;
            }

            EventBus.Interact(interactableId);
            OnClicked?.Invoke();
        }

        /// <summary>
        /// Called by InteractionRaycaster when cursor enters this object.
        /// </summary>
        public void HoverEnter()
        {
            if (!CanInteract()) return;

            isHovered = true;
            highlightEffect?.SetHighlight(true);
            CursorManager.Instance?.SetInteractCursor();
            EventBus.HoverEnter(interactableId);
        }

        /// <summary>
        /// Called by InteractionRaycaster when cursor exits this object.
        /// </summary>
        public void HoverExit()
        {
            isHovered = false;
            highlightEffect?.SetHighlight(false);
            CursorManager.Instance?.SetDefaultCursor();
            EventBus.HoverExit(interactableId);
        }

        private bool CanInteract()
        {
            if (hasBeenUsed && oneTimeUse) return false;

            if (requiresPuzzleSolved && !string.IsNullOrEmpty(requiredPuzzleId))
            {
                if (GameManager.Instance == null) return false;
                if (!GameManager.Instance.IsPuzzleSolved(requiredPuzzleId))
                {
                    return false;
                }
            }

            return true;
        }

        private void HandlePuzzleSolved(string puzzleId)
        {
            // If a gating puzzle was solved, refresh visual state
            if (puzzleId == requiredPuzzleId && requiresPuzzleSolved)
            {
                // Could trigger an "unlocked" animation here
                Debug.Log($"[Interactable] '{interactableId}' is now unlocked (gate '{puzzleId}' solved).");
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(interactableId))
            {
                interactableId = gameObject.name;
            }
        }
    }
}
