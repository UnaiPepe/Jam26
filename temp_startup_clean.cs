using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class StartupSequence : MonoBehaviour
    {
        [Header("Audio Order")]
        [Tooltip("The initial intro sound.")]
        public AudioClip introClip;

        [Tooltip("The loopable sound to play after intro finishes.")]
        public AudioClip mainLoopClip;

        [Tooltip("The background music that loops continuously after the main clip.")]
        public AudioClip backgroundMusic;

        [Header("Ambient Audio")]
        [Tooltip("Secondary audio track that plays alongside background music.")]
        public AudioClip ambientClip;

        [Tooltip("Volume of the ambient track (relative to music).")]
        [Range(0f, 1f)]
        public float ambientVolume = 0.3f;

        [Tooltip("Duration of the fade-in for the ambient track.")]
        public float ambientFadeInTime = 2.0f;

        [Header("Activation")]
        [Tooltip("The Main Menu Text object (M.A.S.K) to activate after intro clip.")]
        public GameObject targetObject;

        private AudioSource _audioSource;
        private AudioSource _ambientSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();

            // Create a second AudioSource for ambient
            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.playOnAwake = false;
            _ambientSource.loop = true;
        }

        private void Start()
        {
            StartCoroutine(PlaySequence());
        }

        private IEnumerator PlaySequence()
        {
            // 1. Play Intro
            if (introClip != null)
            {
                _audioSource.loop = false;
                _audioSource.clip = introClip;
                _audioSource.Play();

                // Wait for the clip to finish
                yield return new WaitForSeconds(introClip.length);
            }

            // 2. Activate Target (The Text) AND Play Main Clip SIMULTANEOUSLY
            if (targetObject != null)
            {
                targetObject.SetActive(true);
            }

            if (mainLoopClip != null)
            {
                _audioSource.clip = mainLoopClip;
                _audioSource.loop = false;
                _audioSource.Play();
            }

            // 3. Wait for Main Clip to finish before BG Music
            if (mainLoopClip != null)
            {
                yield return new WaitForSeconds(mainLoopClip.length);
            }

            // 4. Play Background Music AND Ambient - LOOP
            if (backgroundMusic != null)
            {
                _audioSource.clip = backgroundMusic;
                _audioSource.loop = true;
                _audioSource.Play();
            }

            if (ambientClip != null)
            {
                _ambientSource.clip = ambientClip;
                _ambientSource.volume = 0f; // Start silent for fade-in
                _ambientSource.Play();
                StartCoroutine(FadeInAmbient());
            }
        }

        private IEnumerator FadeInAmbient()
        {
            float elapsed = 0f;

            while (elapsed < ambientFadeInTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / ambientFadeInTime;
                _ambientSource.volume = Mathf.Lerp(0f, ambientVolume, t);
                yield return null;
            }

            _ambientSource.volume = ambientVolume;
        }
    }
}

