using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PathArrowRenderer : MonoBehaviour
{
    [Header("Body")]
    public float width = 0.3f;
    public float height = 0.05f;

    [Header("Arrow Head")]
    public GameObject arrowHeadPrefab;

    private Mesh mesh;
    private GameObject arrowHeadInstance;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "PathArrowMesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // ================= PUBLIC API =================

    public void RenderPath(List<Vector2Int> path)
    {
        if (path == null || path.Count < 2)
        {
            Clear();
            return;
        }

        BuildRibbonMesh(path);
        PlaceArrowHead(path);
    }

    public void Clear()
    {
        mesh.Clear();

        if (arrowHeadInstance != null)
            arrowHeadInstance.SetActive(false);
    }

    // ================= MESH =================

    private void BuildRibbonMesh(List<Vector2Int> path)
    {
        mesh.Clear();

        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();

        Vector3 prev = GridManager.Instance.GridToWorld(path[0]);

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 curr = GridManager.Instance.GridToWorld(path[i]);

            Vector3 dir = (curr - prev).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, dir) * (width * 0.5f);

            Vector3 leftVert = prev - right + Vector3.up * height;
            Vector3 rightVert = prev + right + Vector3.up * height;

            vertices.Add(leftVert);
            vertices.Add(rightVert);

            float v = i / (float)(path.Count - 1);
            uvs.Add(new Vector2(0, v));
            uvs.Add(new Vector2(1, v));

            if (i > 1)
            {
                int baseIndex = vertices.Count - 4;

                triangles.Add(baseIndex + 0);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 1);

                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);
            }

            prev = curr;
        }

        // último punto
        Vector3 lastDir = (GridManager.Instance.GridToWorld(path[^1]) -
                           GridManager.Instance.GridToWorld(path[^2])).normalized;
        Vector3 lastRight = Vector3.Cross(Vector3.up, lastDir) * (width * 0.5f);

        Vector3 end = GridManager.Instance.GridToWorld(path[^1]);
        vertices.Add(end - lastRight + Vector3.up * height);
        vertices.Add(end + lastRight + Vector3.up * height);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        int lastBase = vertices.Count - 4;
        triangles.Add(lastBase + 0);
        triangles.Add(lastBase + 2);
        triangles.Add(lastBase + 1);

        triangles.Add(lastBase + 1);
        triangles.Add(lastBase + 2);
        triangles.Add(lastBase + 3);

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
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
