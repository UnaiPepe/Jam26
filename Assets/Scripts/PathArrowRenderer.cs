using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PathArrowRenderer : MonoBehaviour
{
    [Header("Body")]
    public float width = 0.3f;
    public float height = 0.05f;

    [Header("Prefabs")]
    public GameObject arrowHeadPrefab;
    public GameObject cornerPrefab;

    private Mesh mesh;
    private GameObject arrowHeadInstance;
    private List<GameObject> corners = new();

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "PathArrowMesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // ================= PUBLIC API =================

    public void RenderPath(List<Vector2Int> path)
    {
        Clear();

        if (path == null || path.Count < 2)
            return;

        BuildBodyWithCorners(path);
        PlaceArrowHead(path);
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

    private void BuildBodyWithCorners(List<Vector2Int> path)
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();

        int vertIndex = 0;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2Int from = path[i];
            Vector2Int to = path[i + 1];

            Vector3 a = GridManager.Instance.GridToWorld(from);
            Vector3 b = GridManager.Instance.GridToWorld(to);

            Vector3 dir = (b - a).normalized;
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

            // ===== CORNER DETECTION =====
            if (i < path.Count - 2)
            {
                Vector2Int nextDir = path[i + 2] - to;
                Vector2Int currDir = to - from;

                if (currDir != nextDir)
                {
                    PlaceCorner(to, currDir, nextDir);
                }
            }
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
        GameObject corner = Instantiate(cornerPrefab, transform);
        Vector3 pos = GridManager.Instance.GridToWorld(gridPos);
        pos.y += height;
        corner.transform.position = pos;

        float rot = GetCornerRotation(fromDir, toDir);
        corner.transform.rotation = Quaternion.Euler(90f, rot, 0f);

        corners.Add(corner);
    }

    private float GetCornerRotation(Vector2Int from, Vector2Int to)
    {
        // Base sprite: RIGHT -> DOWN
        if (from == Vector2Int.right && to == Vector2Int.down) return 0;
        if (from == Vector2Int.down && to == Vector2Int.left) return 90;
        if (from == Vector2Int.left && to == Vector2Int.up) return 180;
        if (from == Vector2Int.up && to == Vector2Int.right) return 270;

        if (from == Vector2Int.down && to == Vector2Int.right) return 270;
        if (from == Vector2Int.left && to == Vector2Int.down) return 180;
        if (from == Vector2Int.up && to == Vector2Int.left) return 90;
        if (from == Vector2Int.right && to == Vector2Int.up) return 0;

        return 0;
    }

    // ================= ARROW HEAD =================

    private void PlaceArrowHead(List<Vector2Int> path)
    {
        if (arrowHeadInstance == null)
            arrowHeadInstance = Instantiate(arrowHeadPrefab, transform);

        Vector3 end = GridManager.Instance.GridToWorld(path[^1]);
        Vector3 prev = GridManager.Instance.GridToWorld(path[^2]);

        Vector3 dir = (end - prev).normalized;

        arrowHeadInstance.transform.position =
            end + Vector3.up * height;

        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        arrowHeadInstance.transform.rotation =
            Quaternion.Euler(90f, angle, 0f);

        arrowHeadInstance.SetActive(true);
    }
}
