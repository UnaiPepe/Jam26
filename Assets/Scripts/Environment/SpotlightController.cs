using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class SpotlightController : MonoBehaviour
    {
        [Header("Timing Settings")]
        [Tooltip("Time in seconds between Circus modes.")]
        public float circusInterval = 5.0f;

        [Tooltip("How long the Circus mode lasts.")]
        public float circusDuration = 2.0f;

        [Header("Movement Settings")]
        [Tooltip("Rotation speed in degrees per second.")]
        public float rotationSpeed = 45.0f;

        [Tooltip("Maximum angle (in degrees) the light can rotate from its initial position.")]
        public float maxAngle = 30.0f;

        [Header("Idle Settings")]
        [Tooltip("If true, the light gently wobbles when not in Circus mode.")]
        public bool enableIdleWobble = true;

        [Tooltip("Speed of the idle wobble.")]
        public float wobbleSpeed = 0.5f;

        [Tooltip("Magnitude of the idle wobble (degrees).")]
        public float wobbleAmount = 5.0f;

        // Internal State
        private Quaternion _initialRotation;
        private Quaternion _targetRotation;
        private bool _isCircusMode = false;
        private float _seed; // Random seed for perlin noise uniqueness

        private void Awake()
        {
            _initialRotation = transform.localRotation;
            _targetRotation = _initialRotation;
            _seed = UnityEngine.Random.value * 100f;
        }

        private void Start()
        {
            StartCoroutine(BehaviorLoop());
        }

        private void Update()
        {
            // 1. Determine Target
            if (_isCircusMode)
            {
                // In Circus mode, if we reached the target closely, pick a new random one
                if (Quaternion.Angle(transform.localRotation, _targetRotation) < 1.0f)
                {
                    PickRandomTarget();
                }
            }
            else
            {
                // In Idle mode, target is initial + optional wobble
                if (enableIdleWobble)
                {
                    float xNoise = (Mathf.PerlinNoise(Time.time * wobbleSpeed, _seed) - 0.5f) * wobbleAmount;
                    float yNoise = (Mathf.PerlinNoise(Time.time * wobbleSpeed, _seed + 50f) - 0.5f) * wobbleAmount;
                    
                    // Combine offset with initial rotation
                    // We rotate the initial rotation by these small local offsets
                    Quaternion offset = Quaternion.Euler(xNoise, yNoise, 0);
                    _targetRotation = _initialRotation * offset;
                }
                else
                {
                    _targetRotation = _initialRotation;
                }
            }

            // 2. Move towards Target
            // RotateTowards ensures we move at constant speed and STOP exactly at target (no asymptotic creeping)
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, _targetRotation, rotationSpeed * Time.deltaTime);
        }

        private IEnumerator BehaviorLoop()
        {
            while (true)
            {
                // IDLE PHASE
                _isCircusMode = false;
                yield return new WaitForSeconds(circusInterval);

                // CIRCUS PHASE
                _isCircusMode = true;
                PickRandomTarget(); // Pick first target immediately
                yield return new WaitForSeconds(circusDuration);
            }
        }

        private void PickRandomTarget()
        {
            // Generate a random rotation within 'maxAngle' of '_initialRotation'
            // Simple way: Random inside unit circle projected to angles
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle * maxAngle;
            Quaternion randomOffset = Quaternion.Euler(randomDir.x, randomDir.y, 0);
            
            _targetRotation = _initialRotation * randomOffset;
        }

        [ContextMenu("Test Circus Mode")]
        public void TestCircus()
        {
            StopAllCoroutines();
            _isCircusMode = true;
            PickRandomTarget();
            // Just stay in circus for testing, or restart loop
            StartCoroutine(BehaviorLoop());
            _isCircusMode = true; // Force it back on override
        }
    }
}
