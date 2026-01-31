using System.Collections.Generic;
using UnityEngine;

public class PathArrowRenderer : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject arrowBodyPrefab;
    public GameObject arrowCornerPrefab;
    public GameObject arrowHeadPrefab;

    [Header("Visual")]
    public float height = 0.05f;

    private readonly List<GameObject> spawned = new();

    // ================= PUBLIC API =================

    // start = grid de la unidad
    // path = pasos SIN incluir el destino final
    public void RenderPath(Vector2Int start, List<Vector2Int> path)
    {
        Clear();

        if (path == null || path.Count == 0)
            return;

        Vector2Int prevDir = Vector2Int.zero;
        Vector2Int from = start;

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int to = path[i];
            Vector2Int dir = to - from;

            GameObject piece;

            // ¿Es esquina?
            if (i > 0 && dir != prevDir)
            {
                piece = Instantiate(arrowCornerPrefab, transform);
                piece.transform.rotation = GetCornerRotation(prevDir, dir);
            }
            else
            {
                piece = Instantiate(arrowBodyPrefab, transform);
                piece.transform.rotation = GetStraightRotation(dir);
            }

            Vector3 worldPos = GridManager.Instance.GridToWorld(from);
            worldPos.y += height;
            piece.transform.position = worldPos;

            spawned.Add(piece);

            prevDir = dir;
            from = to;
        }

        // Flecha final (punta)
        SpawnArrowHead(from, prevDir);
    }

    public void Clear()
    {
        foreach (GameObject go in spawned)
        {
            if (go != null)
                Destroy(go);
        }

        spawned.Clear();
    }

    // ================= ROTATIONS =================

    private Quaternion GetStraightRotation(Vector2Int dir)
    {
        // SIEMPRE X = 90
        if (dir == Vector2Int.right) return Quaternion.Euler(90f, 0f, 0f);
        if (dir == Vector2Int.down) return Quaternion.Euler(90f, 90f, 0f);
        if (dir == Vector2Int.left) return Quaternion.Euler(90f, 180f, 0f);
        if (dir == Vector2Int.up) return Quaternion.Euler(90f, 270f, 0f);

        return Quaternion.Euler(90f, 0f, 0f);
    }

    private Quaternion GetCornerRotation(Vector2Int from, Vector2Int to)
    {
        // SIEMPRE X = 90

        // Right -> Down
        if (from == Vector2Int.right && to == Vector2Int.down)
            return Quaternion.Euler(90f, 90f, 0f);

        // Down -> Right
        if (from == Vector2Int.down && to == Vector2Int.right)
            return Quaternion.Euler(90f, 90f, 0f);

        // Left -> Up
        if (from == Vector2Int.left && to == Vector2Int.up)
            return Quaternion.Euler(90f, 270f, 0f);

        // Up -> Left
        if (from == Vector2Int.up && to == Vector2Int.left)
            return Quaternion.Euler(90f, 180f, 0f);

        // Right -> Up
        if (from == Vector2Int.right && to == Vector2Int.up)
            return Quaternion.Euler(90f, 270f, 0f);

        // Up -> Right
        if (from == Vector2Int.up && to == Vector2Int.right)
            return Quaternion.Euler(90f, 0f, 0f);

        // Left -> Down
        if (from == Vector2Int.left && to == Vector2Int.down)
            return Quaternion.Euler(90f, 90f, 0f);

        // Down -> Left
        if (from == Vector2Int.down && to == Vector2Int.left)
            return Quaternion.Euler(90f, 180f, 0f);

        return Quaternion.Euler(90f, 0f, 0f);
    }

    // ================= ARROW HEAD =================

    private void SpawnArrowHead(Vector2Int gridPos, Vector2Int dir)
    {
        if (arrowHeadPrefab == null)
            return;

        GameObject head = Instantiate(arrowHeadPrefab, transform);
        head.transform.rotation = GetStraightRotation(dir);

        Vector3 pos = GridManager.Instance.GridToWorld(gridPos);
        pos.y += height;
        head.transform.position = pos;

        spawned.Add(head);
    }
}
