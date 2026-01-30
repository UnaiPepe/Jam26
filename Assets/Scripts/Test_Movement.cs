using System.Collections;
using UnityEngine;

public class Test_Movement : MonoBehaviour
{
    public float cellSize = 1f;
    public float stepDuration = 0.12f;

    public int gridWidth = 15;   // X: 0..14
    public int gridHeight = 15;  // Z: 0..14

    // gridPos.x = X, gridPos.y = Z
    public Vector2Int gridPos;

    public bool IsMoving { get; private set; }

    void Start()
    {
        gridPos = WorldToGrid(transform.position);
        gridPos = ClampToBounds(gridPos);
        transform.position = GridToWorld(gridPos);
    }

    public void MoveTo(Vector2Int targetGrid)
    {
        if (IsMoving) return;
        targetGrid = ClampToBounds(targetGrid);
        StartCoroutine(MovePathRoutine(targetGrid));
    }

    IEnumerator MovePathRoutine(Vector2Int targetGrid)
    {
        IsMoving = true;

        Vector2Int dir = targetGrid - gridPos;

        // Debe ser línea recta
        Vector2Int stepDir;
        int steps;

        if (dir.x != 0 && dir.y == 0)
        {
            stepDir = new Vector2Int(dir.x > 0 ? 1 : -1, 0);
            steps = Mathf.Abs(dir.x);
        }
        else if (dir.y != 0 && dir.x == 0)
        {
            stepDir = new Vector2Int(0, dir.y > 0 ? 1 : -1);
            steps = Mathf.Abs(dir.y);
        }
        else
        {
            IsMoving = false;
            yield break;
        }

        for (int i = 0; i < steps; i++)
        {
            Vector2Int next = gridPos + stepDir;
            if (!IsInsideBounds(next)) break;

            Vector3 start = transform.position;
            Vector3 target = GridToWorld(next);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / stepDuration;
                transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }

            transform.position = target;
            gridPos = next;
        }

        IsMoving = false;
    }

    public bool IsInsideBounds(Vector2Int gp)
    {
        return gp.x >= 0 && gp.x < gridWidth && gp.y >= 0 && gp.y < gridHeight;
    }

    public Vector2Int ClampToBounds(Vector2Int gp)
    {
        gp.x = Mathf.Clamp(gp.x, 0, gridWidth - 1);
        gp.y = Mathf.Clamp(gp.y, 0, gridHeight - 1);
        return gp;
    }

    public Vector3 GridToWorld(Vector2Int gp)
    {
        return new Vector3(gp.x * cellSize, transform.position.y, gp.y * cellSize);
    }

    public Vector2Int WorldToGrid(Vector3 world)
    {
        int x = Mathf.RoundToInt(world.x / cellSize);
        int z = Mathf.RoundToInt(world.z / cellSize);
        return new Vector2Int(x, z);
    }
}
