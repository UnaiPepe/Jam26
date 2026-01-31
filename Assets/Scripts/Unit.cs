using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int movementRange = 5;
    public Vector2Int GridPosition { get; private set; }

    private void Start()
    {
        GridPosition = GridManager.Instance.WorldToGrid(transform.position);
    }

    private void OnMouseDown()
    {
        Debug.Log("CLICK EN UNIDAD");
        MovementPreview.Instance.Show(this);
    }

    public IEnumerator MoveTo(Vector2Int destination)
    {
        List<Vector2Int> path = Pathfinder.FindPath(GridPosition, destination);

        foreach (Vector2Int step in path)
        {
            Vector3 targetPos = GridManager.Instance.GridToWorld(step);

            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    4f * Time.deltaTime
                );
                yield return null;
            }
        }

        GridPosition = destination;
    }
}
