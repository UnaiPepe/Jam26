using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Phases
{
    [RequireComponent(typeof(TMP_Text))]
    public class PhaseAnnouncement : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("Speed when entering.")]
        public float entranceSpeed = 2500f;
        
        [Tooltip("Speed when exiting.")]
        public float exitSpeed = 2500f;

        [Tooltip("Duration to stay in center.")]
        public float stayDuration = 1.5f;

        [Header("Drift & Vibrate")]
        [Tooltip("Slow movement speed while in the center.")]
        public float centerDriftSpeed = 50f;
        
        [Tooltip("Intensity of the vibration/shake while in the center.")]
        public float vibrationIntensity = 5.0f;

        [Tooltip("Speed of the vibration (Frequency).")]
        public float vibrationSpeed = 20f;

        [Header("Impact & Shake")]
        [Tooltip("How long the screen shakes.")]
        public float shakeDuration = 0.15f;
        
        [Tooltip("Strength of the shake.")]
        public float shakeMagnitude = 0.3f;

        [Header("Events")]
        [Tooltip("Invoked when the text slams into the center. Good for SFX.")]
        public UnityEvent OnPhaseImpact;

        [Header("Audio")]
        [Tooltip("Primary Sound clip to play when this announcement appears.")]
        public AudioClip announcementClip;
        [Range(0f, 1f)]
        public float volume1 = 1.0f;

        [Tooltip("Secondary Sound clip to play simultaneously.")]
        public AudioClip announcementClip2;
        [Range(0f, 1f)]
        public float volume2 = 1.0f;
        
        private AudioSource _audioSource;

        private TMP_Text _textComponent;
        private RectTransform _rectTransform;
        private Vector3 _originalPosition;
        private float _canvasWidth;
        private UnityEngine.Camera _mainCamera;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
            _rectTransform = GetComponent<RectTransform>();
            _originalPosition = _rectTransform.localPosition; 
            _mainCamera = UnityEngine.Camera.main;
            
            // Try get AudioSource on this object, or Add one if needed (though usually user adds it)
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) 
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }
        }
        
        private void OnEnable()
        {
            RectTransform parentRect = transform.parent as RectTransform;
            if (parentRect != null)
                _canvasWidth = parentRect.rect.width;
            else
                _canvasWidth = Screen.width; 

            // Play Audio
            if (_audioSource != null)
            {
                if (announcementClip != null) _audioSource.PlayOneShot(announcementClip, volume1);
                if (announcementClip2 != null) _audioSource.PlayOneShot(announcementClip2, volume2);
            }

            StartCoroutine(AnimatePhaseImpact());
        }

        private IEnumerator AnimatePhaseImpact()
        {
            float textWidth = _rectTransform.rect.width;
            
            // Linear Start/End
            float startX = -(_canvasWidth / 2f) - textWidth;
            float endX = (_canvasWidth / 2f) + textWidth;
            float centerX = 0f;

            // Reset
            _rectTransform.localPosition = new Vector3(startX, _originalPosition.y, _originalPosition.z);
            _rectTransform.localRotation = Quaternion.identity; // No rotation

            // 1. Entrance: Fast Linear
            while (_rectTransform.localPosition.x < centerX)
            {
                _rectTransform.localPosition += Vector3.right * entranceSpeed * Time.deltaTime;
                yield return null;
            }

            // 2. IMPACT!
            _rectTransform.localPosition = new Vector3(centerX, _originalPosition.y, _originalPosition.z);

            // Trigger Shake & Events
            OnPhaseImpact?.Invoke();
            
            // Use CameraManager for shake if available
            if (Assets.Scripts.Camera.CameraManager.Instance != null)
            {
                Assets.Scripts.Camera.CameraManager.Instance.Shake(shakeDuration, shakeMagnitude);
            }

            // 3. Stay + Vibrate
            float timer = 0f;
            while (timer < stayDuration)
            {
                timer += Time.deltaTime;
                
                // Drift
                _rectTransform.localPosition += Vector3.right * centerDriftSpeed * Time.deltaTime;

                // Vibrate
                VibrateText();
                
                yield return null;
            }

            // 4. Exit: Fast Linear
            while (_rectTransform.localPosition.x < endX)
            {
                _rectTransform.localPosition += Vector3.right * exitSpeed * Time.deltaTime;
                yield return null;
            }

            // Deactivate parent (the container GO that PhaseManager references)
            if (transform.parent != null)
            {
                transform.parent.gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
            _rectTransform.localPosition = _originalPosition;
        }

        private void VibrateText()
        {
            _textComponent.ForceMeshUpdate();
            var textInfo = _textComponent.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;
                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                // Use Perlin noise for "controlled speed" vibration (Wobble/Shake)
                // Use vertexIndex as offset so each vertex moves differently
                // Subtract 0.5 to center the noise between -0.5 and 0.5
                float xNoise = Mathf.PerlinNoise(Time.time * vibrationSpeed, vertexIndex * 0.2f) - 0.5f;
                float yNoise = Mathf.PerlinNoise(Time.time * vibrationSpeed, (vertexIndex * 0.2f) + 50f) - 0.5f;

                // Scale by intensity (x2 because range is 0.5, creating full unit range)
                Vector3 jitterOffset = new Vector3(xNoise, yNoise, 0) * vibrationIntensity * 2f;

                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] += jitterOffset;
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                _textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}
