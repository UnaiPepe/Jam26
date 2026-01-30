using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class GridSystem : MonoBehaviour
{

    [SerializeField] GameObject objectToPlace;
    [SerializeField] int gridSize;
    GameObject ghostObject;
    [SerializeField] HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();

    private void Start()
    {
        CreateGhostObject();
    }

    private void Update()
    {
        UpdateGhostPosition();

        if(Input.GetMouseButtonDown(0))
        {
            PlaceObject();
        }
    }

    // Method to create the GameObject
    void CreateGhostObject()
    {
        ghostObject = Instantiate(objectToPlace);
        ghostObject.GetComponent<Collider>().enabled = false;

        Renderer[] renderers = ghostObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            Color color = mat.color;
            color.a = 0.5f;

            mat.color = color;


            // Change the spawned object color
            mat.SetFloat(";Mode", 2);
            mat.SetInt("_ScreBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }
    
    //Method to update the ghost position
    void UpdateGhostPosition()
    {
        // Raycast from the mouse
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // If the Raycast Hit
        if(Physics.Raycast(ray, out RaycastHit hit))
        {

            // We transform the hit position to snap to the Grid 
            Vector3 point = hit.point;
            Vector3 snappedPosition = new Vector3(
                Mathf.Round(point.x / gridSize) * gridSize,
                Mathf.Round(point.y / gridSize) * gridSize,
                Mathf.Round(point.z / gridSize) * gridSize
                );

            ghostObject.transform.position = snappedPosition;

            if (occupiedPositions.Contains(snappedPosition))
            {
                SetGhostColor(Color.red);
            }
            else
            {
                SetGhostColor(new Color(1f,1f, 1f, 0.5f));
            }
        }
    }

    void SetGhostColor(Color color)
    {
        Renderer[] renderers=ghostObject.GetComponentsInChildren<Renderer>();

        foreach(Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            mat.color = color;
        }
    }

    void PlaceObject()
    {
        Vector3 placementPosition=ghostObject.transform.position;

        if (!occupiedPositions.Contains(placementPosition))
        {
            Instantiate(objectToPlace, placementPosition, Quaternion.identity);

            occupiedPositions.Add(placementPosition);
        }
    }
}
