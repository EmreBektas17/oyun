using UnityEngine;
using PuzzleGame.Transition;

namespace PuzzleGame.Interaction
{
    /// <summary>
    /// Attach to room hotspots that should open a zoom view when clicked.
    /// Wire Interactable.OnClicked -> TriggerZoom().
    /// </summary>
    public class ZoomTrigger : MonoBehaviour
    {
        [Tooltip("The viewId of the ZoomViewController to open.")]
        public string zoomViewId;

        public void TriggerZoom()
        {
            if (TransitionManager.Instance != null && !string.IsNullOrEmpty(zoomViewId))
            {
                TransitionManager.Instance.OpenZoomView(zoomViewId);
            }
        }
    }
}
