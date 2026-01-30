using System.Collections.Generic;
using UnityEngine;

public class MovementPreview : MonoBehaviour
{
    public static MovementPreview Instance;

    public GameObject tilePrefab;
    public GameObject ghostPrefab;

    private Unit selectedUnit;
    private GameObject ghost;
    private List<GameObject> tiles = new();

    private Vector2Int selectedDestination;
    public Vector2Int GetSelectedDestination()
    {
        return selectedDestination;
    }

    private void Awake()
    {
        Instance = this;
        ghost = Instantiate(ghostPrefab);
        ghost.SetActive(false);
    }

    private void Update()
    {
        if (selectedUnit == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector2Int gridPos = GridManager.Instance.WorldToGrid(hit.point);

            if (GridManager.Instance.IsInsideGrid(gridPos))
            {
                ghost.SetActive(true);
                ghost.transform.position = GridManager.Instance.GridToWorld(gridPos);
            }

            if (Input.GetMouseButtonDown(0))
            {
                selectedDestination = gridPos;
            }
        }
    }

    public void Show(Unit unit)
    {
        Clear();
        selectedUnit = unit;

        for (int x = -unit.movementRange; x <= unit.movementRange; x++)
        {
            for (int y = -unit.movementRange; y <= unit.movementRange; y++)
            {
                Vector2Int pos = unit.GridPosition + new Vector2Int(x, y);
                if (!GridManager.Instance.IsInsideGrid(pos)) continue;
                if (Mathf.Abs(x) + Mathf.Abs(y) > unit.movementRange) continue;

                GameObject tile = Instantiate(tilePrefab,
                    GridManager.Instance.GridToWorld(pos),
                    Quaternion.identity);

                tiles.Add(tile);
            }
        }
    }

    public void Clear()
    {
        foreach (var t in tiles) Destroy(t);
        tiles.Clear();
        ghost.SetActive(false);
    }

    public void ConfirmMove()
    {
        if (selectedUnit == null) return;

        StartCoroutine(
            selectedUnit.MoveTo(selectedDestination)
        );

        Clear();
    }


}
