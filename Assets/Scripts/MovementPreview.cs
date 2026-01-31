using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementPreview : MonoBehaviour
{
    public static MovementPreview Instance;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject ghostPrefab;

    [Header("Line Settings")]
    public float lineHeight = 0.1f;
    public float lineWidth = 0.15f;

    // ===== ESTADO POR UNIDAD =====
    private Dictionary<Unit, GhostData> unitGhosts = new();

    private Unit selectedUnit;
    private bool ignoreCancelThisFrame = false;

    // ===== TILES ACTIVOS (solo del unit seleccionado) =====
    private List<GameObject> tiles = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (selectedUnit == null)
            return;

        if (ignoreCancelThisFrame)
        {
            ignoreCancelThisFrame = false;
            return;
        }

        // Click derecho → deseleccionar unidad (ghost y línea permanecen)
        if (Input.GetMouseButtonDown(1))
        {
            DeselectUnit();
            return;
        }

        // Click izquierdo fuera de tile → deseleccionar
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsMouseOverTile())
                DeselectUnit();
        }
    }

    // ===== SELECCIONAR UNIDAD =====
    public void Show(Unit unit)
    {
        ClearTiles();

        selectedUnit = unit;
        ignoreCancelThisFrame = true;

        if (!unitGhosts.ContainsKey(unit))
        {
            CreateGhostDataForUnit(unit);
        }

        Vector2Int origin = unit.GridPosition;

        for (int x = -unit.movementRange; x <= unit.movementRange; x++)
        {
            for (int y = -unit.movementRange; y <= unit.movementRange; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > unit.movementRange)
                    continue;

                Vector2Int gridPos = origin + new Vector2Int(x, y);
                if (!GridManager.Instance.IsInsideGrid(gridPos))
                    continue;

                Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);
                worldPos.y = 0f;

                GameObject tileGO = Instantiate(
                    tilePrefab,
                    worldPos,
                    Quaternion.Euler(90f, 0f, 0f)
                );

                tileGO.GetComponent<MoveTile>().Init(gridPos);
                tiles.Add(tileGO);
            }
        }
    }

    // ===== GHOST PREVIEW =====
    public void ShowGhost(Vector2Int gridPos)
    {
        GhostData data = unitGhosts[selectedUnit];
        if (data.hasDestination)
            return;

        data.ghost.SetActive(true);

        Vector3 pos = GridManager.Instance.GridToWorld(gridPos);
        pos.y = data.ghost.transform.position.y;
        data.ghost.transform.position = pos;

        UpdatePathLine(selectedUnit, gridPos);
    }

    public void HideGhost()
    {
        GhostData data = unitGhosts[selectedUnit];
        if (data.hasDestination)
            return;

        data.ghost.SetActive(false);
        data.line.positionCount = 0;
    }

    // ===== SELECCIONAR DESTINO =====
    public void SelectDestination(Vector2Int gridPos)
    {
        GhostData data = unitGhosts[selectedUnit];

        data.hasDestination = true;
        data.destination = gridPos;

        data.ghost.SetActive(true);

        Vector3 pos = GridManager.Instance.GridToWorld(gridPos);
        pos.y = data.ghost.transform.position.y;
        data.ghost.transform.position = pos;

        UpdatePathLine(selectedUnit, gridPos);
    }

    // ===== CONFIRMAR MOVIMIENTO =====
    public void ConfirmMove()
    {
        foreach (var pair in unitGhosts)
        {
            Unit unit = pair.Key;
            GhostData data = pair.Value;

            if (!data.hasDestination)
                continue;

            // Mover la unidad
            StartCoroutine(unit.MoveTo(data.destination));

            // Limpiar estado visual
            data.hasDestination = false;
            data.line.positionCount = 0;
            data.ghost.SetActive(false);
        }

        // Limpiar tiles y selección
        ClearTiles();
        selectedUnit = null;
    }

    // ===== PATH LINE =====
    private void UpdatePathLine(Unit unit, Vector2Int destination)
    {
        GhostData data = unitGhosts[unit];

        List<Vector2Int> path = Pathfinder.FindPath(
            unit.GridPosition,
            destination
        );

        data.line.positionCount = path.Count + 1;

        Vector3 startPos = GridManager.Instance.GridToWorld(unit.GridPosition);
        startPos.y = lineHeight;
        data.line.SetPosition(0, startPos);

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pos = GridManager.Instance.GridToWorld(path[i]);
            pos.y = lineHeight;
            data.line.SetPosition(i + 1, pos);
        }
    }

    // ===== DESELECCIONAR =====
    private void DeselectUnit()
    {
        ClearTiles();
        selectedUnit = null;
    }

    // ===== LIMPIAR TILES =====
    private void ClearTiles()
    {
        foreach (GameObject t in tiles)
            Destroy(t);

        tiles.Clear();
    }

    // ===== CREAR GHOST + LINE POR UNIDAD =====
    private void CreateGhostDataForUnit(Unit unit)
    {
        GameObject ghost = Instantiate(ghostPrefab);
        ghost.SetActive(false);

        GameObject lineGO = new GameObject($"PathLine_{unit.name}");
        LineRenderer line = lineGO.AddComponent<LineRenderer>();

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.useWorldSpace = true;
        line.positionCount = 0;

        unitGhosts[unit] = new GhostData
        {
            ghost = ghost,
            line = line,
            hasDestination = false
        };
    }

    // ===== UTIL =====
    private bool IsMouseOverTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponent<MoveTile>() != null;
        }

        return false;
    }
}

// ===== DATOS POR UNIDAD =====
public class GhostData
{
    public GameObject ghost;
    public LineRenderer line;
    public Vector2Int destination;
    public bool hasDestination;
}
