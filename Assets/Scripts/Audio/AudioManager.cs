using UnityEngine;
using PuzzleGame.Core;

namespace PuzzleGame.Audio
{
    /// <summary>
    /// Centralized audio manager. Handles SFX and ambient audio.
    /// Provides methods for playing one-shot sounds triggered by game events.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;

        [Header("Common SFX")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip solveSound;
        [SerializeField] private AudioClip failSound;
        [SerializeField] private AudioClip doorOpenSound;
        [SerializeField] private AudioClip transitionSound;

        [Header("Ambient")]
        [SerializeField] private AudioClip ambientLoop;

        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float ambientVolume = 0.3f;

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
            EventBus.OnInteract += HandleInteract;
            EventBus.OnHoverEnter += HandleHoverEnter;
            EventBus.OnPuzzleSolved += HandlePuzzleSolved;
            EventBus.OnPuzzleAttemptFailed += HandlePuzzleFailed;
            EventBus.OnGameComplete += HandleGameComplete;
            EventBus.OnZoomEnter += HandleZoomTransition;
        }

        private void OnDisable()
        {
            EventBus.OnInteract -= HandleInteract;
            EventBus.OnHoverEnter -= HandleHoverEnter;
            EventBus.OnPuzzleSolved -= HandlePuzzleSolved;
            EventBus.OnPuzzleAttemptFailed -= HandlePuzzleFailed;
            EventBus.OnGameComplete -= HandleGameComplete;
            EventBus.OnZoomEnter -= HandleZoomTransition;
        }

        private void Start()
        {
            // Start ambient
            if (ambientLoop != null && ambientSource != null)
            {
                ambientSource.clip = ambientLoop;
                ambientSource.loop = true;
                ambientSource.volume = ambientVolume;
                ambientSource.Play();
            }
        }

        // --- Public API ---

        public void PlaySFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }

        public void PlaySFX(AudioClip clip, float volumeScale)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volumeScale);
            }
        }

        // --- Event Handlers ---

        private void HandleInteract(string id)
        {
            PlaySFX(clickSound);
        }

        private void HandleHoverEnter(string id)
        {
            PlaySFX(hoverSound, 0.3f);
        }

        private void HandlePuzzleSolved(string id)
        {
            PlaySFX(solveSound);
        }

        private void HandlePuzzleFailed(string id)
        {
            PlaySFX(failSound);
        }

        private void HandleGameComplete()
        {
            PlaySFX(doorOpenSound);
        }

        private void HandleZoomTransition(string viewId)
        {
            PlaySFX(transitionSound, 0.5f);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
