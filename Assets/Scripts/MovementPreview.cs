using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovementPreview : MonoBehaviour
{
    public static MovementPreview Instance;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject ghostPrefab;
    public GameObject arrowHeadPrefab;
    public Material arrowBodyMaterial;

    private Unit selectedUnit;

    private Dictionary<Unit, GhostData> unitGhosts = new();
    private List<GameObject> tiles = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (TurnManager.Instance.IsExecutingMovement())
        {
            HideAllPreviews();
            return;
        }

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

        Unit unit = hit.collider.GetComponent<Unit>();
        if (unit != null)
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

        data.arrow.RenderPath(path);
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

        GameObject arrowGO = new GameObject($"Arrow_{unit.name}");
        arrowGO.transform.SetParent(transform);

        var arrow = arrowGO.AddComponent<PathArrowRenderer>();
        arrow.arrowHeadPrefab = arrowHeadPrefab;

        arrowGO.GetComponent<MeshRenderer>().material = arrowBodyMaterial;

        unitGhosts[unit] = new GhostData
        {
            ghost = ghost,
            arrow = arrow,
            hasDestination = false
        };
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
