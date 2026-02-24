using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PuzzleGame.Core;
using PuzzleGame.Interaction;

namespace PuzzleGame.Transition
{
    /// <summary>
    /// Manages transitions between the main room view and zoom-in puzzle views.
    /// Orchestrates fade controller + zoom view controllers.
    /// </summary>
    public class TransitionManager : MonoBehaviour
    {
        public static TransitionManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private FadeController fadeController;
        [SerializeField] private InteractionRaycaster roomRaycaster;
        [SerializeField] private GameObject roomRoot;

        [Header("Zoom Views")]
        [SerializeField] private List<ZoomViewController> zoomViews = new List<ZoomViewController>();

        [Header("Settings")]
        [SerializeField] private float transitionDuration = 0.3f;

        private ZoomViewController activeZoomView;
        private bool isTransitioning;

        public bool IsInZoomView => activeZoomView != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnZoomEnter += HandleZoomEnter;
            EventBus.OnZoomExit += HandleZoomExit;
        }

        private void OnDisable()
        {
            EventBus.OnZoomEnter -= HandleZoomEnter;
            EventBus.OnZoomExit -= HandleZoomExit;
        }

        /// <summary>
        /// Open a zoom view by its ID.
        /// </summary>
        public void OpenZoomView(string viewId)
        {
            EventBus.ZoomEnter(viewId);
        }

        /// <summary>
        /// Close the current zoom view and return to room.
        /// Called by Back buttons on zoom views.
        /// </summary>
        public void CloseZoomView()
        {
            EventBus.ZoomExit();
        }

        private void HandleZoomEnter(string viewId)
        {
            if (isTransitioning) return;

            ZoomViewController target = FindZoomView(viewId);
            if (target == null)
            {
                Debug.LogWarning($"[TransitionManager] Zoom view '{viewId}' not found.");
                return;
            }

            StartCoroutine(TransitionToZoom(target));
        }

        private void HandleZoomExit()
        {
            if (isTransitioning || activeZoomView == null) return;

            StartCoroutine(TransitionToRoom());
        }

        private IEnumerator TransitionToZoom(ZoomViewController target)
        {
            isTransitioning = true;

            // Disable room interaction
            if (roomRaycaster != null)
            {
                roomRaycaster.SetActive(false);
            }

            // Fade out
            yield return fadeController.FadeOut(transitionDuration);

            // Hide room, show zoom view
            if (roomRoot != null)
            {
                roomRoot.SetActive(false);
            }

            activeZoomView = target;
            activeZoomView.Show();

            // Fade in
            yield return fadeController.FadeIn(transitionDuration);

            // Re-enable raycaster for zoom view puzzle interactions
            if (roomRaycaster != null)
            {
                roomRaycaster.SetActive(true);
            }

            isTransitioning = false;
        }

        private IEnumerator TransitionToRoom()
        {
            isTransitioning = true;

            // Fade out
            yield return fadeController.FadeOut(transitionDuration);

            // Hide zoom view, show room
            activeZoomView.Hide();
            activeZoomView = null;

            if (roomRoot != null)
            {
                roomRoot.SetActive(true);
            }

            // Fade in
            yield return fadeController.FadeIn(transitionDuration);

            // Re-enable room interaction
            if (roomRaycaster != null)
            {
                roomRaycaster.SetActive(true);
            }

            isTransitioning = false;
        }

        private ZoomViewController FindZoomView(string viewId)
        {
            foreach (var view in zoomViews)
            {
                if (view.ViewId == viewId) return view;
            }
            return null;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
