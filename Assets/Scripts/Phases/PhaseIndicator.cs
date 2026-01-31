using TMPro;
using UnityEngine;

namespace Assets.Scripts.Phases
{
    [RequireComponent(typeof(TMP_Text))]
    public class PhaseIndicator : MonoBehaviour
    {
        [Header("Wave Settings")]
        [Tooltip("How fast the wave travels through the text.")]
        public float waveSpeed = 5.0f;

        [Tooltip("Height of the wave (vertical displacement).")]
        public float waveHeight = 10.0f;

        [Tooltip("Distance between wave peaks. Lower value makes it wider/smoother.")]
        public float waveFrequency = 0.5f;

        [Header("Tremble Settings")]
        [Tooltip("Magnitude of the vertex jitter/tremble.")]
        public float trembleMagnitude = 2.0f;

        [Tooltip("Speed of the tremble (Frequency).")]
        public float trembleSpeed = 20f;

        private TMP_Text _textComponent;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            UpdateWave();
        }

        private void UpdateWave()
        {
            _textComponent.ForceMeshUpdate();
            TMP_TextInfo textInfo = _textComponent.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;
                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                // Wave Calculation:
                // Using (Time - Index) creates a wave that moves from index 0 to N.
                // This will look like it starts at the top (if vertical) and moves down.
                float argument = (Time.time * waveSpeed) - (i * waveFrequency);
                float offsetY = Mathf.Sin(argument) * waveHeight;
                Vector3 waveOffset = new Vector3(0, offsetY, 0);

                // Apply to all 4 vertices of the character
                for (int j = 0; j < 4; j++)
                {
                    // Tremble Calculation with Perlin Noise for speed control
                    float xNoise = Mathf.PerlinNoise(Time.time * trembleSpeed, vertexIndex * 0.2f + j) - 0.5f;
                    float yNoise = Mathf.PerlinNoise(Time.time * trembleSpeed, (vertexIndex * 0.2f + j) + 50f) - 0.5f;

                    Vector3 jitter = new Vector3(xNoise, yNoise, 0) * trembleMagnitude * 2f;

                    vertices[vertexIndex + j] += waveOffset + jitter;
                }
            }

            // Update Mesh
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                _textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}
