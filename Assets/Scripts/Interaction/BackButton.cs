using UnityEngine;
using PuzzleGame.Transition;

namespace PuzzleGame.Interaction
{
    /// <summary>
    /// Attach to Back buttons in zoom views.
    /// Calls TransitionManager to return to the main room.
    /// </summary>
    public class BackButton : MonoBehaviour
    {
        public void OnBackClicked()
        {
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.CloseZoomView();
            }
        }
    }
}
