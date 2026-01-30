using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / cellSize),
            Mathf.RoundToInt(worldPos.z / cellSize)
        );
    }

    public bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < width && pos.y < height;
    }
}
