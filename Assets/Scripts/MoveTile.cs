using UnityEngine;

public class MoveTile : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }

    public void Init(Vector2Int gridPos)
    {
        GridPosition = gridPos;
    }

    private void OnMouseEnter()
    {
        MovementPreview.Instance.ShowGhost(GridPosition);
    }

    private void OnMouseExit()
    {
        MovementPreview.Instance.HideGhost();
    }

    private void OnMouseDown()
    {
        MovementPreview.Instance.SelectDestination(GridPosition);
    }
}
