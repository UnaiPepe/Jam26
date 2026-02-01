using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovementPreview : MonoBehaviour
{
    public static MovementPreview Instance;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject ghostPrefab;

    [Header("Arrow Prefabs")]
    public GameObject arrowBodyPrefab;
    public GameObject arrowCornerPrefab;
    public GameObject arrowHeadPrefab;

    private Unit selectedUnit;

    private Dictionary<Unit, GhostData> unitGhosts = new Dictionary<Unit, GhostData>();
    private List<GameObject> tiles = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (TurnManager.Instance == null)
            return;

        // Bloquea input durante ejecuci?n
        if (TurnManager.Instance.IsExecutingMovement())
        {
            HideAllPreviews();
            return;
        }

        // El NPC no usa input humano
        if (TurnManager.Instance.CurrentTeamTurn == TeamTurn.NPC)
            return;

        // Bloqueo UI
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
            HandleLeftClick();

        if (Input.GetMouseButtonDown(1))
            CancelSelection();
    }

    // ================= INPUT =================

    private void HandleLeftClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            CancelSelection();
            return;
        }

        // Si ya hay unidad seleccionada, elegir destino
        if (selectedUnit != null)
        {
            Vector2Int gridPos =
                GridManager.Instance.WorldToGrid(hit.point);

            if (!GridManager.Instance.IsInsideGrid(gridPos))
                return;

            if (!IsInsideMovementRange(selectedUnit, gridPos))
                return;

            SelectDestination(gridPos);
            return;
        }

        // Selecci?n de unidad
        Unit unit = hit.collider.GetComponent<Unit>();
        if (unit == null)
            return;

        // Solo unidades del equipo activo
        if (!IsUnitSelectableThisTurn(unit))
            return;

        SelectUnit(unit);
    }

    // ================= SELECTION =================

    private void SelectUnit(Unit unit)
    {
        ClearTiles();
        selectedUnit = unit;

        if (!unitGhosts.ContainsKey(unit))
            CreateGhostData(unit);

        ShowMovementTiles(unit);
    }

    private void CancelSelection()
    {
        ClearTiles();
        selectedUnit = null;
    }

    // ================= DESTINATION =================

    private void SelectDestination(Vector2Int gridPos)
    {
        GhostData data = unitGhosts[selectedUnit];

        selectedUnit.SetPlannedDestination(gridPos);

        data.hasDestination = true;
        data.destination = gridPos;

        Vector3 pos = GridManager.Instance.GridToWorld(gridPos);
        pos.y = data.ghost.transform.position.y;
        data.ghost.transform.position = pos;
        data.ghost.SetActive(true);

        List<Vector2Int> path =
            Pathfinder.FindPath(selectedUnit.GridPosition, gridPos);

        if (path.Count >= 2)
            path.RemoveAt(path.Count - 1);

        data.arrow.RenderPath(selectedUnit.GridPosition, path);
    }

    // ================= VISUAL =================

    private void ShowMovementTiles(Unit unit)
    {
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

                Vector3 worldPos =
                    GridManager.Instance.GridToWorld(gridPos);
                worldPos.y = 0f;

                GameObject tile = Instantiate(
                    tilePrefab,
                    worldPos,
                    Quaternion.Euler(90f, 0f, 0f)
                );

                tiles.Add(tile);
            }
        }
    }

    private void ClearTiles()
    {
        foreach (GameObject t in tiles)
            Destroy(t);

        tiles.Clear();
    }

    private void HideAllPreviews()
    {
        ClearTiles();
        selectedUnit = null;

        foreach (var kvp in unitGhosts)
        {
            kvp.Value.ghost.SetActive(false);
            kvp.Value.arrow.Clear();
        }
    }

    // ================= HELPERS =================

    private bool IsUnitSelectableThisTurn(Unit unit)
    {
        TeamTurn current = TurnManager.Instance.CurrentTeamTurn;

        if (current == TeamTurn.Jugador1 && unit.team == Unit.Team.Jugador1)
            return true;

        if (current == TeamTurn.Jugador2 && unit.team == Unit.Team.Jugador2)
            return true;

        return false;
    }

    private bool IsInsideMovementRange(Unit unit, Vector2Int gridPos)
    {
        Vector2Int origin = unit.GridPosition;
        int dist =
            Mathf.Abs(gridPos.x - origin.x) +
            Mathf.Abs(gridPos.y - origin.y);

        return dist <= unit.movementRange;
    }

    private void CreateGhostData(Unit unit)
    {
        GameObject ghost = Instantiate(ghostPrefab);
        ghost.SetActive(false);

        GameObject arrowGO = new GameObject("Arrow_" + unit.name);
        arrowGO.transform.SetParent(transform);

        PathArrowRenderer arrow = arrowGO.AddComponent<PathArrowRenderer>();
        arrow.arrowBodyPrefab = arrowBodyPrefab;
        arrow.arrowCornerPrefab = arrowCornerPrefab;
        arrow.arrowHeadPrefab = arrowHeadPrefab;

        unitGhosts[unit] = new GhostData
        {
            ghost = ghost,
            arrow = arrow,
            hasDestination = false
        };
    }

    // ================= AI SUPPORT =================

    public void AI_ShowGhostAndArrow(Unit unit, Vector2Int gridPos)
    {
        if (unit == null)
            return;

        if (!unitGhosts.ContainsKey(unit))
            CreateGhostData(unit);

        GhostData data = unitGhosts[unit];

        unit.SetPlannedDestination(gridPos);
        data.hasDestination = true;
        data.destination = gridPos;

        Vector3 pos = GridManager.Instance.GridToWorld(gridPos);
        pos.y = data.ghost.transform.position.y;
        data.ghost.transform.position = pos;
        data.ghost.SetActive(true);

        List<Vector2Int> path =
            Pathfinder.FindPath(unit.GridPosition, gridPos);

        if (path.Count >= 2)
            path.RemoveAt(path.Count - 1);

        data.arrow.RenderPath(unit.GridPosition, path);
    }

    public void AI_HideGhostAndArrow(Unit unit)
    {
        if (unit == null)
            return;

        if (!unitGhosts.ContainsKey(unit))
            return;

        GhostData data = unitGhosts[unit];
        data.hasDestination = false;
        data.ghost.SetActive(false);
        data.arrow.Clear();
    }
}

// ================= DATA =================

public class GhostData
{
    public GameObject ghost;
    public PathArrowRenderer arrow;
    public Vector2Int destination;
    public bool hasDestination;
}
