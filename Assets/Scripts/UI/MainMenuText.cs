using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class MainMenuText : MonoBehaviour
    {

        [Header("Animation Settings")]
        [Tooltip("Delay before the animation starts (for loading).")]
        public float initialDelay = 1.0f;

        [Tooltip("Delay between each letter appearing.")]
        public float appearDelay = 0.3f;
        
        [Tooltip("Scale multiplier when a letter pops in.")]
        public float popScale = 2.0f;
        
        [Tooltip("How fast the letter scales back to normal.")]
        public float popDuration = 0.2f;

        [Header("Curvature Settings")]
        [Tooltip("Strength of the curve (Positive = Smiley, Negative = Frown).")]
        public float curveStrength = 10.0f;

        [Header("Vibration Settings")]
        [Tooltip("Amount of vertex jitter.")]
        public float vibrationIntensity = 2.0f;

        [Tooltip("Speed of the vibration.")]
        public float vibrationSpeed = 20f;

        [Header("Shake Settings")]
        [Tooltip("Camera shake duration per letter.")]
        public float shakeDuration = 0.1f;
        
        [Tooltip("Camera shake strength per letter.")]
        public float shakeMagnitude = 0.2f;

        [Header("Events")]
        [Tooltip("Invoked when the intro sequence finishes.")]
        public UnityEvent OnIntroComplete;

        private TMP_Text _textComponent;
        private UnityEngine.Camera _mainCamera;
        
        // Track the visibility/scale of each character for the intro
        private float[] _charScales;
        private bool _introComplete = false;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
            _mainCamera = UnityEngine.Camera.main;
        }

        private void Start()
        {
            // Prepare text info
            _textComponent.ForceMeshUpdate();
            int charCount = _textComponent.textInfo.characterCount;
            _charScales = new float[charCount];

            // Start invisible
            for (int i = 0; i < charCount; i++) _charScales[i] = 0f;

            StartCoroutine(IntroSequence());
        }

        private void Update()
        {
            ApplyEffects();
        }

        private IEnumerator IntroSequence()
        {
            // Initial Wait
            yield return new WaitForSeconds(initialDelay);

            int charCount = _textComponent.textInfo.characterCount;

            for (int i = 0; i < charCount; i++)
            {
                // Verify visibility (skip spaces if any, though M.A.S.K usually filters them or counts them)
                if (!_textComponent.textInfo.characterInfo[i].isVisible) 
                {
                    _charScales[i] = 1f; // Just show invisible chars immediately to keep logic simple
                    continue; 
                }

                // 1. Pop In
                StartCoroutine(AnimateCharPop(i));
                
                // 2. Shake Camera
                StartCoroutine(ShakeCamera());

                // 3. Wait
                yield return new WaitForSeconds(appearDelay);
            }

            _introComplete = true;
            OnIntroComplete?.Invoke();
        }

        private IEnumerator AnimateCharPop(int index)
        {
            float timer = 0f;
            
            // Pop Up (0 -> PopScale) very fast? Or just start at PopScale?
            // "Al aparecer... se hará más grande de lo que ya es" imply it appears BIG then shrinks.
            
            while (timer < popDuration)
            {
                timer += Time.deltaTime;
                float t = timer / popDuration;
                
                // Lerp from PopScale to 1.0
                _charScales[index] = Mathf.Lerp(popScale, 1f, t);
                yield return null;
            }
            _charScales[index] = 1f;
        }

        private void ApplyEffects()
        {
            _textComponent.ForceMeshUpdate();
            TMP_TextInfo textInfo = _textComponent.textInfo;
            int charCount = textInfo.characterCount;

            // Safe check if charScales is not initialized yet
            if (_charScales == null || _charScales.Length != charCount) return;

            // Compute curve center reference (middle of the text)
            // Bounds of the text object
            float boundsMinX = _textComponent.bounds.min.x;
            float boundsMaxX = _textComponent.bounds.max.x;
            float boundsWidth = boundsMaxX - boundsMinX;

            for (int i = 0; i < charCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;
                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                // --- 1. Calculate Intro Scale & Vibration ---
                Vector3 center = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;
                float currentScale = _charScales[i];

                // If intro is not complete, characters scale from 0 to 1 (or pop)
                // If intro is complete, they are 1. 
                // However, they *always* vibrate.

                // Create transformation matrix for Scale
                Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(currentScale, currentScale, 1));

                for (int j = 0; j < 4; j++)
                {
                    Vector3 original = vertices[vertexIndex + j];
                    
                    // A. Scale relative to char center
                    Vector3 centered = original - center;
                    Vector3 scaled = matrix.MultiplyPoint3x4(centered);
                    Vector3 result = center + scaled;

                    // B. Apply Curve (Parabola)
                    // Offset Y based on distance from text center
                    // Formula: y -= (x - center)^2 * strength
                    // Normalize x position relative to text width (-0.5 to 0.5)
                    float relX = (result.x - (boundsMinX + boundsWidth / 2f)) / boundsWidth; 
                    float curveOffset = -(relX * relX * curveStrength * 100f); // *100 arbitrary multiplier to make slider snappy
                    result.y += curveOffset;

                    // C. Apply Vibration (Tremble)
                    float xNoise = Mathf.PerlinNoise(Time.time * vibrationSpeed, vertexIndex * 0.2f + j) - 0.5f;
                    float yNoise = Mathf.PerlinNoise(Time.time * vibrationSpeed, (vertexIndex * 0.2f + j) + 50f) - 0.5f;
                    Vector3 jitter = new Vector3(xNoise, yNoise, 0) * vibrationIntensity;

                    vertices[vertexIndex + j] = result + jitter;
                }
            }

            // Upload
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                _textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }

        private IEnumerator ShakeCamera()
        {
            if (_mainCamera == null) yield break;

            Vector3 originalCamPos = _mainCamera.transform.position;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float strength = Mathf.Lerp(shakeMagnitude, 0f, elapsed / shakeDuration);
                
                Vector3 randomPoint = originalCamPos + Random.insideUnitSphere * strength;
                randomPoint.z = originalCamPos.z; 

                _mainCamera.transform.position = randomPoint;

                yield return null;
            }
            _mainCamera.transform.position = originalCamPos;
        }
    }
}
