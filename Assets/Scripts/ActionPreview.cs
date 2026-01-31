using UnityEngine;
using System.Collections.Generic;

public class ActionPreview : MonoBehaviour
{
    public static ActionPreview Instance;

    [Header("Preview Tile Prefab (attack area)")]
    public GameObject tilePrefab;

    [Header("Direction Tile Prefab (4 choices)")]
    public GameObject directionTilePrefab;
   

    private List<GameObject> tiles = new();

    public Vector2Int SelectedDir { get; private set; } = Vector2Int.right;
    public bool HasDirection { get; private set; } = false;

    Unit currentUnit;
    bool pickingDirection = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!pickingDirection) return;

        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
            TryPickDirectionTile();
    }

    public void EnterAction(Unit unit)
    {
        currentUnit = unit;
        HasDirection = false;
        SelectedDir = Vector2Int.right;

        pickingDirection = true;
        DrawDirectionChoices();
    }

    public void ExitAction()
    {
        pickingDirection = false;
        Clear();
        currentUnit = null;
        HasDirection = false;
    }

    // Llamar desde UI cuando elijas dirección
    public void SetDirection(Vector2Int dir)
    {
        SelectedDir = dir;
        HasDirection = true;
        pickingDirection = false;
        DrawPreview();
    }
    void TryPickDirectionTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        var dirTile = hit.collider.GetComponent<AttackDirectionTile>();
        if (dirTile == null) return;

        SetDirection(dirTile.dir);
    }

    void DrawDirectionChoices()
    {
        Clear();
        if (currentUnit == null) return;
        if (!currentUnit.HasPlannedMovement) return;

        Vector2Int origin = currentUnit.PlannedDestination;

        Vector2Int[] dirs = new[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

        foreach (var d in dirs)
        {
            Vector2Int gp = origin + d;
            if (!GridManager.Instance.IsInsideGrid(gp)) continue;

            Vector3 w = GridManager.Instance.GridToWorld(gp);
            w.y = 0f;

            GameObject t = Instantiate(directionTilePrefab, w, Quaternion.Euler(90, 0, 0));
            tiles.Add(t);

            var comp = t.GetComponent<AttackDirectionTile>();
            if (comp == null) comp = t.AddComponent<AttackDirectionTile>();
            comp.dir = d;
        }
    }


    void DrawPreview()
    {
        Clear();
        if (currentUnit == null) return;
        if (!currentUnit.HasPlannedMovement) return;

        Vector2Int origin = currentUnit.PlannedDestination;

        List<Vector2Int> area = new()
        {
            origin + SelectedDir * 1,
            origin + SelectedDir * 2,
            origin + SelectedDir * 3
        };

        foreach (var gp in area)
        {
            if (!GridManager.Instance.IsInsideGrid(gp)) continue;

            Vector3 w = GridManager.Instance.GridToWorld(gp);
            w.y = 0f;

            GameObject t = Instantiate(tilePrefab, w, Quaternion.Euler(90, 0, 0));
            tiles.Add(t);
        }
    }

    void Clear()
    {
        foreach (var t in tiles) Destroy(t);
        tiles.Clear();
    }
}
