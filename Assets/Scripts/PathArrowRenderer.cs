using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PathArrowRenderer : MonoBehaviour
{
    [Header("Body")]
    public float width = 0.2f;     // grosor del cuerpo
    public float height = 0.3f;    // altura sobre el suelo

    [Header("Prefabs")]
    public GameObject arrowHeadPrefab;
    public GameObject cornerPrefab;

    private Mesh mesh;
    private GameObject arrowHeadInstance;
    private readonly List<GameObject> corners = new();

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "PathArrowMesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // ================= PUBLIC API =================

    // start = casilla de la unidad
    // path = pasos HASTA ANTES del fantasma
    public void RenderPath(Vector2Int start, List<Vector2Int> path)
    {
        Clear();

        if (path == null || path.Count == 0)
            return;

        BuildBodyAndCorners(start, path);
        PlaceArrowHead(start, path);
    }

    public void Clear()
    {
        mesh.Clear();

        if (arrowHeadInstance != null)
            arrowHeadInstance.SetActive(false);

        foreach (var c in corners)
            Destroy(c);
        corners.Clear();
    }

    // ================= BODY + CORNERS =================

    private void BuildBodyAndCorners(Vector2Int start, List<Vector2Int> path)
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();

        int vertIndex = 0;
        float shrink = 0.015f; // pequeño, para no crear huecos

        Vector2Int from = start;

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int to = path[i];

            Vector3 a = GridManager.Instance.GridToWorld(from);
            Vector3 b = GridManager.Instance.GridToWorld(to);

            Vector3 dir = (b - a).normalized;

            a += dir * shrink;
            b -= dir * shrink;

            Vector3 right = Vector3.Cross(Vector3.up, dir) * (width * 0.5f);

            Vector3 v0 = a - right + Vector3.up * height;
            Vector3 v1 = a + right + Vector3.up * height;
            Vector3 v2 = b - right + Vector3.up * height;
            Vector3 v3 = b + right + Vector3.up * height;

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            triangles.Add(vertIndex + 0);
            triangles.Add(vertIndex + 2);
            triangles.Add(vertIndex + 1);

            triangles.Add(vertIndex + 1);
            triangles.Add(vertIndex + 2);
            triangles.Add(vertIndex + 3);

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));

            vertIndex += 4;

            // ===== DETECTAR ESQUINA =====
            if (i < path.Count - 1)
            {
                Vector2Int next = path[i + 1];

                Vector2Int dirA = to - from;
                Vector2Int dirB = next - to;

                if (dirA != dirB)
                    PlaceCorner(to, dirA, dirB);
            }

            from = to;
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    // ================= CORNER =================

    private void PlaceCorner(
        Vector2Int gridPos,
        Vector2Int fromDir,
        Vector2Int toDir)
    {
        if (cornerPrefab == null)
            return;

        GameObject corner = Instantiate(cornerPrefab, transform);
        Vector3 pos = GridManager.Instance.GridToWorld(gridPos);
        pos.y += height;
        corner.transform.position = pos;

        float rot = GetCornerRotation(fromDir, toDir);
        corner.transform.rotation = Quaternion.Euler(90f, rot, 0f);

        corners.Add(corner);
    }

    // Tu sprite base es: UP -> RIGHT
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

    // ================= ARROW HEAD =================

    private void PlaceArrowHead(Vector2Int start, List<Vector2Int> path)
    {
        if (arrowHeadPrefab == null || path.Count == 0)
            return;

        if (arrowHeadInstance == null)
            arrowHeadInstance = Instantiate(arrowHeadPrefab, transform);

        Vector2Int last = path[^1];
        Vector2Int prev = (path.Count >= 2) ? path[^2] : start;

        Vector3 end = GridManager.Instance.GridToWorld(last);
        Vector3 before = GridManager.Instance.GridToWorld(prev);

        Vector3 dir = (end - before).normalized;

        arrowHeadInstance.transform.position =
            end + Vector3.up * height;

        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        arrowHeadInstance.transform.rotation =
            Quaternion.Euler(90f, angle, 0f);

        arrowHeadInstance.SetActive(true);
    }
}
