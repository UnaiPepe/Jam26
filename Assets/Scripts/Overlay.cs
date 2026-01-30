using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class Overlay : MonoBehaviour
{
    [Header("UI")]
    public GameObject overlayPanel;
    public Button avanzarButton;

    [Header("Board Raycast")]
    public LayerMask boardLayer;     // pon aquí la layer Board
    public float maxRayDistance = 200f;

    [Header("Preview")]
    public GameObject previewMarkerPrefab; // opcional, si no pones nada, no muestra marker

    Test_Movement currentMover;
    GameObject currentCharacter;

    bool isTargeting = false;
    bool moveConsumed = false; // hasta volver a clicar al personaje

    GameObject previewMarker;

    void Awake()
    {
        overlayPanel.SetActive(false);

        avanzarButton.onClick.AddListener(() =>
        {
            if (currentMover == null) return;
            if (moveConsumed) return;
            if (currentMover.IsMoving) return;

            // Entramos en modo apuntar
            isTargeting = true;
        });
    }

    public void OpenOverlayFor(GameObject character)
    {
        currentCharacter = character;
        currentMover = character.GetComponent<Test_Movement>();

        // Si ya gastaste el movimiento, al volver a clicar lo “recargas”
        moveConsumed = false;
        isTargeting = false;

        overlayPanel.SetActive(true);
    }

    void Update()
    {
        if (!overlayPanel.activeSelf) return;
        if (currentMover == null) return;

        // Si no estamos apuntando, no hacemos nada
        if (!isTargeting) return;

        // Raycast al tablero
        if (!TryGetHoveredGridCell(out Vector2Int hoveredCell))
        {
            HidePreview();
            return;
        }

        // Comprobamos si la celda está a 1-3 en línea recta
        if (IsValidMoveTarget(currentMover.gridPos, hoveredCell, out int steps))
        {
            ShowPreviewAt(hoveredCell);

            // Click para confirmar
            if (Input.GetMouseButtonDown(0))
            {
                // mueve y termina turno
                currentMover.MoveTo(hoveredCell);
                moveConsumed = true;
                isTargeting = false;
                overlayPanel.SetActive(false);
                HidePreview();
            }
        }
        else
        {
            HidePreview();
        }
    }

    bool TryGetHoveredGridCell(out Vector2Int cell)
    {
        cell = default;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, boardLayer))
        {
            // Convertimos el punto a celda usando el GridMover
            // (cellSize lo leemos del mover para que sea consistente)
            float size = currentMover.cellSize;

            int x = Mathf.RoundToInt(hit.point.x / size);
            int z = Mathf.RoundToInt(hit.point.z / size);

            cell = new Vector2Int(x, z);
            return true;
        }

        return false;
    }

    bool IsValidMoveTarget(Vector2Int from, Vector2Int to, out int steps)
    {
        steps = 0;

        Vector2Int d = to - from;

        // Debe ser línea recta
        if (d.x != 0 && d.y == 0)
        {
            steps = Mathf.Abs(d.x);
        }
        else if (d.y != 0 && d.x == 0)
        {
            steps = Mathf.Abs(d.y);
        }
        else
        {
            return false;
        }

        // Máximo 3
        if (steps < 1 || steps > 3) return false;

        // Dentro del tablero
        if (!currentMover.IsInsideBounds(to)) return false;

        return true;
    }

    void ShowPreviewAt(Vector2Int cell)
    {
        if (previewMarkerPrefab == null) return;

        if (previewMarker == null)
            previewMarker = Instantiate(previewMarkerPrefab);

        // Coloca el marker en esa celda, un pelín arriba del suelo
        Vector3 pos = new Vector3(
            cell.x * currentMover.cellSize,
            0.02f,
            cell.y * currentMover.cellSize
        );

        previewMarker.transform.position = pos;
    }

    void HidePreview()
    {
        if (previewMarker != null)
            previewMarker.SetActive(false);
    }
}
