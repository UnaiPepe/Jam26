using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    public int movementRange = 5;

    // Posicion en el grid
    public Vector2Int GridPosition { get; private set; }

    // Planificacion (usada por MovementExecution)
    public bool HasPlannedMovement { get; private set; }
    public Vector2Int PlannedDestination { get; private set; }

    private bool isMoving;

    private void Start()
    {
        GridPosition = GridManager.Instance.WorldToGrid(transform.position);
        transform.position = GridManager.Instance.GridToWorld(GridPosition);
    }

    // ================= PLANIFICACION =================
    public void SetPlannedDestination(Vector2Int destination)
    {
        PlannedDestination = destination;
        HasPlannedMovement = true;
    }

    public void ClearPlannedMovement()
    {
        HasPlannedMovement = false;
    }

    // ================= MOVIMIENTO =================
    public bool IsMoving()
    {
        return isMoving;
    }

    // Llamado por MovementExecution
    public void MoveOneStep(Vector2Int targetGridPos, System.Action onFinished)
    {
        if (isMoving)
            return;

        Vector3 targetWorldPos =
            GridManager.Instance.GridToWorld(targetGridPos);

        StartCoroutine(
            MoveCoroutine(targetGridPos, targetWorldPos, onFinished)
        );
    }

    private IEnumerator MoveCoroutine(
        Vector2Int targetGridPos,
        Vector3 targetWorldPos,
        System.Action onFinished)
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 end = targetWorldPos;

        float distance = Vector3.Distance(start, end);
        float duration = distance / moveSpeed;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
        GridPosition = targetGridPos;

        isMoving = false;
        onFinished?.Invoke();
    }
}
