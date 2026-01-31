using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class HoverMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuration")]
    [Tooltip("El objeto que contiene los botones hijos que se deben mostrar/ocultar.")]
    public GameObject childButtonsContainer;

    [Tooltip("Tiempo de espera antes de ocultar el menú al salir del puntero (evita parpadeos).")]
    public float hideDelay = 0.2f;

    [Header("Animation")]
    [Tooltip("Tiempo que tarda en crecer el menú al aparecer.")]
    public float scaleUpDuration = 0.2f;
    [Tooltip("Tiempo que tarda el fade out al desaparecer.")]
    public float fadeOutDuration = 0.15f;

    private Coroutine hideCoroutine;
    private Coroutine showCoroutine;
    private CanvasGroup childCanvasGroup;

    private void Start()
    {
        if (childButtonsContainer != null)
        {
            // Intentamos obtener el CanvasGroup, si no existe lo añadimos.
            // Es necesario para controlar la opacidad (Fade).
            childCanvasGroup = childButtonsContainer.GetComponent<CanvasGroup>();
            if (childCanvasGroup == null)
            {
                childCanvasGroup = childButtonsContainer.AddComponent<CanvasGroup>();
            }

            // Estado inicial: oculto
            childButtonsContainer.SetActive(false);
            childButtonsContainer.transform.localScale = Vector3.one * 0.1f;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (childButtonsContainer != null)
        {
            // Solo activamos y animamos si NO estaba ya activo
            if (!childButtonsContainer.activeSelf)
            {
                childButtonsContainer.SetActive(true);
                if (showCoroutine != null) StopCoroutine(showCoroutine);
                showCoroutine = StartCoroutine(AnimateShow());
            }
            else
            {
                // Si ya estaba activo (porque cancelamos el hide), nos aseguramos de que esté visible
                if (childCanvasGroup != null) childCanvasGroup.alpha = 1f;
                childButtonsContainer.transform.localScale = Vector3.one;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Cancelar animación de mostrar si estaba en curso y pasar a ocultar con delay
        // No cancelamos showCoroutine aquí inmediatamente para permitir movimientos rápidos, 
        // pero el hideCoroutine se encargará de la transición.
        hideCoroutine = StartCoroutine(HideMenuAndAnimate());
    }

    private IEnumerator AnimateShow()
    {
        // Resetear valores iniciales para la animación de entrada
        childButtonsContainer.transform.localScale = Vector3.one * 0.1f;
        if (childCanvasGroup != null) childCanvasGroup.alpha = 1f; // Asegurar visible

        float elapsedTime = 0f;
        Vector3 startScale = Vector3.one * 0.1f;
        Vector3 endScale = Vector3.one;

        while (elapsedTime < scaleUpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scaleUpDuration;
            // Efecto EaseOutBack simple para el "pop"
            // Fórmula: c1 * t * t * t - c3 * t * t (aprox) o simplemente un curve suave.
            // Usamos SmoothStep para simplicidad o una curva personalizada si se desea.
            childButtonsContainer.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        childButtonsContainer.transform.localScale = endScale;
        showCoroutine = null;
    }

    private IEnumerator HideMenuAndAnimate()
    {
        // Esperar el delay (para evitar parpadeos si sale y entra rápido)
        yield return new WaitForSeconds(hideDelay);

        // Animación de salida: Fade a negro/transparente muy rápido
        if (childCanvasGroup != null)
        {
            float elapsedTime = 0f;
            float startAlpha = childCanvasGroup.alpha;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeOutDuration;
                childCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
            childCanvasGroup.alpha = 0f;
        }

        if (childButtonsContainer != null)
        {
            childButtonsContainer.SetActive(false);
            // Resetear escala para la próxima vez (opcional, ya se hace en AnimateShow)
            childButtonsContainer.transform.localScale = Vector3.one * 0.1f;
        }
        hideCoroutine = null;
    }
}
