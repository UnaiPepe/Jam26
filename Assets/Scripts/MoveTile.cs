using UnityEngine;

public class MoveTile : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }

    public void Init(Vector2Int gridPos)
    {
        GridPosition = gridPos;
    }
}
