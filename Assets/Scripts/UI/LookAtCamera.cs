using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [Tooltip("Invierte la dirección (útil si el objeto aparece al revés). Para UI/Texto suele ser mejor dejarlo en false si usamos forward=forward.")]
    public bool invertForward = false;

    private Transform cameraTransform;

    void Start()
    {
        // Cacheamos el transform de la cámara principal para mejorar rendimiento
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("LookAtCamera: No se encontró 'Camera.main'. Asegúrate de que tu cámara tenga la etiqueta 'MainCamera'.");
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Modo Billboard:
        // Para que un Canvas WorldSpace se lea, normalmente necesitamos que su forward apunte IGUAL que la cámara.
        // O sea: transform.forward = cameraTransform.forward
        
        if (invertForward)
        {
             transform.forward = -cameraTransform.forward;
        }
        else
        {
            transform.forward = cameraTransform.forward;
        }
    }
}
