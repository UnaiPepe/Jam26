using System.Collections.Generic;
using UnityEngine;

public class PathArrowRenderer : MonoBehaviour
{
    [Header("Visual")]
    public float height = 0.05f;

    [Header("Prefabs")]
    public GameObject arrowBodyPrefab;
    public GameObject arrowCornerPrefab;
    public GameObject arrowHeadPrefab;

    private readonly List<GameObject> spawned = new();

    // ================= PUBLIC API =================

    // start = posición inicial de la unidad
    // path  = lista de celdas HASTA el destino (sin incluir la celda de la unidad)
    public void RenderPath(Vector2Int start, List<Vector2Int> path)
    {
        Clear();

        if (path == null || path.Count == 0)
            return;

        Vector2Int from = start;

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int current = path[i];
            Vector2Int dir = current - from;

            bool isLast = i == path.Count - 1;
            bool hasTurn = false;

            if (!isLast)
            {
                Vector2Int nextDir = path[i + 1] - current;
                hasTurn = nextDir != dir;
            }

            if (isLast)
            {
                PlaceArrowHead(current, dir);
            }
            else if (hasTurn)
            {
                PlaceCorner(current, dir, path[i + 1] - current);
            }
            else
            {
                PlaceBody(current, dir);
            }

            from = current;
        }
    }

    public void Clear()
    {
        foreach (var go in spawned)
            Destroy(go);

        spawned.Clear();
    }

    // ================= PLACEMENT =================

    private void PlaceBody(Vector2Int gridPos, Vector2Int dir)
    {
        GameObject body = Instantiate(arrowBodyPrefab, transform);
        SetupTransform(body, gridPos, GetDirAngle(dir));
    }

    private void PlaceArrowHead(Vector2Int gridPos, Vector2Int dir)
    {
        GameObject head = Instantiate(arrowHeadPrefab, transform);
        SetupTransform(head, gridPos, GetDirAngle(dir));
    }

    private void PlaceCorner(Vector2Int gridPos, Vector2Int fromDir, Vector2Int toDir)
    {
        GameObject corner = Instantiate(arrowCornerPrefab, transform);

        float rot;
        bool flipX;

        GetCornerTransform(fromDir, toDir, out rot, out flipX);

        SetupTransform(corner, gridPos, rot);

        if (flipX)
        {
            Vector3 s = corner.transform.localScale;
            s.x *= -1f;
            corner.transform.localScale = s;
        }
    }

    private void GetCornerTransform( Vector2Int from, Vector2Int to, out float rotY, out bool flipX)
    {
        flipX = false;

        // Giro horario
        if (from == Vector2Int.up && to == Vector2Int.right) { rotY = 0f; return; }
        if (from == Vector2Int.right && to == Vector2Int.down) { rotY = 90f; return; }
        if (from == Vector2Int.down && to == Vector2Int.left) { rotY = 180f; return; }
        if (from == Vector2Int.left && to == Vector2Int.up) { rotY = 270f; return; }

        // Giro antihorario (NECESITA FLIP)
        if (from == Vector2Int.right && to == Vector2Int.up) { rotY = 0f; flipX = true; return; }
        if (from == Vector2Int.down && to == Vector2Int.right) { rotY = 90f; flipX = true; return; }
        if (from == Vector2Int.left && to == Vector2Int.down) { rotY = 180f; flipX = true; return; }
        if (from == Vector2Int.up && to == Vector2Int.left) { rotY = 270f; flipX = true; return; }

        rotY = 0f;
    }


    private void SetupTransform(GameObject go, Vector2Int gridPos, float rotY)
    {
        go.transform.position =
            GridManager.Instance.GridToWorld(gridPos) + Vector3.up * height;

        go.transform.rotation =
            Quaternion.Euler(90f, rotY, 0f);

        spawned.Add(go);
    }

    // ================= ROTATION =================

    private float GetDirAngle(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return 0f;
        if (dir == Vector2Int.right) return 90f;
        if (dir == Vector2Int.down) return 180f;
        if (dir == Vector2Int.left) return 270f;

        return 0f;
    }

    /*
    // Asume que el sprite base de la esquina es: UP -> RIGHT
    private float GetCornerRotation(Vector2Int from, Vector2Int to)
    {
        if (from == Vector2Int.up && to == Vector2Int.right) return 0f;
        if (from == Vector2Int.right && to == Vector2Int.down) return 90f;
        if (from == Vector2Int.down && to == Vector2Int.left) return 180f;
        if (from == Vector2Int.left && to == Vector2Int.up) return 270f;

        // Curvas inversas
        if (from == Vector2Int.right && to == Vector2Int.up) return 270f;
        if (from == Vector2Int.down && to == Vector2Int.right) return 180f;
        if (from == Vector2Int.left && to == Vector2Int.down) return 90f;
        if (from == Vector2Int.up && to == Vector2Int.left) return 0f;

        return 0f;
    }
    */
}
