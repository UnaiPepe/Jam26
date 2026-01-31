using UnityEngine;

namespace Assets.Scripts.Camera
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance;

        [Header("Target Camera")]
        [Tooltip("The camera to control. If empty, uses Camera.main.")]
        public Transform targetCamera;

        [Header("Views")]
        [Tooltip("The initial top-down menu view.")]
        public Transform menuView;
        [Tooltip("The view for the boxing ring/gameplay.")]
        public Transform ringView;
        
        [Tooltip("List of all views to cycle through.")]
        public Transform[] allViews;

        [Header("Settings")]
        [Tooltip("Time it takes to reach the target view (SmoothTime).")]
        public float transitionTime = 1.0f;
        
        [Tooltip("If true, the camera rotates to match the target view.")]
        public bool matchRotation = true;

        private Transform _currentView;
        private int _currentViewIndex = 0;

        // Movement variables
        private Vector3 _currentVelocity;
        
        // Shake variables
        private float _shakeTimer;
        private float _shakeTotalDuration;
        private float _startUnscaledMagnitude;
        private Vector3 _basePosition; // Tracks logical position without shake
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            if (targetCamera == null && UnityEngine.Camera.main != null)
            {
                targetCamera = UnityEngine.Camera.main.transform;
            }
        }

        private void Start()
        {
            // Auto-populate allViews if empty
            if ((allViews == null || allViews.Length == 0) && menuView != null && ringView != null)
            {
                allViews = new Transform[] { menuView, ringView };
                Debug.Log("CameraManager: Auto-populated allViews with menuView and ringView.");
            }

            // Start at the menu view if assigned
            if (menuView != null)
            {
                MoveToView(menuView);
                // Snap immediately to start
                if (targetCamera != null)
                {
                    targetCamera.position = menuView.position;
                    targetCamera.rotation = menuView.rotation;
                    _basePosition = menuView.position;
                }
            }
        }

        private void LateUpdate()
        {
            if (_currentView == null || targetCamera == null) return;

            Vector3 shakeOffset = Vector3.zero;

            // Handle Shake
            if (_shakeTimer > 0)
            {
                _shakeTimer -= Time.deltaTime;

                // Calculate decay (1.0 down to 0.0)
                float progress = Mathf.Max(0, _shakeTimer / _shakeTotalDuration); 
                float currentStrength = Mathf.Lerp(0f, _startUnscaledMagnitude, progress);

                shakeOffset = Random.insideUnitSphere * currentStrength;
            }

            // SmoothDamp the BASE position (Arrives in approximately transitionTime)
            _basePosition = Vector3.SmoothDamp(_basePosition, _currentView.position, ref _currentVelocity, transitionTime);
            
            // Apply shake as temporary visual offset
            targetCamera.position = _basePosition + shakeOffset;

            // Smoothly interpolate rotation
            if (matchRotation)
            {
                // For rotation, Slerp with a factor derived from SmoothDamp is tricky.
                // Simple approach: Use same 'transitionTime' ratio or Quaternion.RotateTowards.
                // However, SmoothDamp effectively creates an ease-out. 
                // We'll use Slerp with a damp factor similar to position or just a simple Slerp that is fast enough.
                // Better approach for "Fixed Time" equivalent in Rotation: Quaternion.Slerp or SmoothDamp equivalent.
                // Let's use Slerp with a dynamic t based on distance to mimic SmoothDamp or just use a standard quick Lerp.
                // The user complained about "avanza mas lento" (asymptotic), SmoothDamp IS asymptotic but optimized for arrival.
                // If the user wants STRICT linear time, we need MoveTowards.
                // But "tarda mucho... avanza mas lento" implies he dislikes the infinite tail of Lerp.
                // SmoothDamp is good. Let's stick with Slerp but faster, or use SmoothDampAngle if we decomposed it.
                // Let's just use Slerp with a slightly higher speed factor relative to transitionTime to ensure it keeps up.
                float rotSpeed = 1f / Mathf.Max(0.01f, transitionTime); // Inverse of time
                targetCamera.rotation = Quaternion.Slerp(targetCamera.rotation, _currentView.rotation, Time.deltaTime * rotSpeed * 3f); 
                // *3f is a heuristic to make rotation feel responsive. 
                // Alternatively, purely linear:
                // targetCamera.rotation = Quaternion.RotateTowards(targetCamera.rotation, _currentView.rotation, (180f / transitionTime) * Time.deltaTime);
            }
        }

        [ContextMenu("Move To Menu")]
        public void MoveToMenu()
        {
            MoveToView(menuView);
        }

        public void Shake(float duration, float magnitude)
        {
            _shakeTotalDuration = duration;
            _shakeTimer = duration;
            _startUnscaledMagnitude = magnitude;
        }

        [ContextMenu("Move To Ring")]
        public void MoveToRing()
        {
            MoveToView(ringView);
        }
        
        [ContextMenu("Test Shake")]
        public void TestShake()
        {
            Shake(0.5f, 1f);
        }

        [ContextMenu("Next View")]
        public void NextView()
        {
            if (allViews == null || allViews.Length == 0)
            {
                Debug.LogWarning("CameraManager: allViews is empty! Assign views in the inspector or add menuView and ringView.");
                return;
            }

            _currentViewIndex = (_currentViewIndex + 1) % allViews.Length;
            Transform nextView = allViews[_currentViewIndex];
            
            Debug.Log($"CameraManager: Moving to view {_currentViewIndex}: {(nextView != null ? nextView.name : "NULL")}");
            MoveToView(nextView);
        }

        public void MoveToView(Transform target)
        {
            _currentView = target;
        }

        /// <summary>
        /// Attempts to find a child named "CameraPos" on the target and move there.
        /// If not found, moves to the target's position with offset logic (if we add that later),
        /// currently just moves to the target transform itself.
        /// </summary>
        public void FocusCharacter(Transform character)
        {
            // Try to find a child specifically for camera positioning
            Transform camPos = character.Find("CameraPos");
            if (camPos != null)
            {
                _currentView = camPos;
            }
            else
            {
                // Fallback: Just look at the character? 
                // Or maybe the user will manually set up Empty parents. 
                // For now, let's treat the character itself as the target 
                // but usually you don't want the camera INSIDE the character.
                // We'll warn if no CameraPos is found.
                Debug.LogWarning($"No 'CameraPos' child found on {character.name}. Moving camera to character origin (might be clipped).");
                _currentView = character;
            }
        }
    }
}
