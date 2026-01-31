using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementExecution : MonoBehaviour
{
    public static MovementExecution Instance;
    public bool IsRunning { get; private set; }

    private class UnitMoveData
    {
        public Unit unit;
        public Queue<Vector2Int> path;
        public bool finished;
    }

    private List<UnitMoveData> units = new List<UnitMoveData>();

    private void Awake()
    {
        Instance = this;
    }

    // Llamado por TurnManager o por un boton
    public void Begin()
    {
        IsRunning = true;
        units.Clear();

        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            if (!u.HasPlannedMovement)
                continue;

            List<Vector2Int> path =
                Pathfinder.FindPath(u.GridPosition, u.PlannedDestination);

            if (path.Count == 0)
                continue;

            units.Add(new UnitMoveData
            {
                unit = u,
                path = new Queue<Vector2Int>(path),
                finished = false
            });
        }

        StartCoroutine(ExecuteMovement());


    }

    public void BeginEnemiesOnly()
    {
        IsRunning = true;
        units.Clear();

        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            // SOLO enemigos (los que tienen el script Enemy)
            if (u.GetComponent<Enemy>() == null)
                continue;

            // Solo si la IA había planeado movimiento
            if (!u.HasPlannedMovement)
                continue;

            List<Vector2Int> path =
                Pathfinder.FindPath(u.GridPosition, u.PlannedDestination);

            if (path == null || path.Count == 0)
                continue;

            units.Add(new UnitMoveData
            {
                unit = u,
                path = new Queue<Vector2Int>(path),
                finished = false
            });
        }

        // Ejecuta el movimiento enemigo
        StartCoroutine(ExecuteMovement());
    }

    private IEnumerator ExecuteMovement()
    {
        bool anyMoving = true;

        while (anyMoving)
        {
            anyMoving = false;

            int movingThisStep = 0;

            foreach (UnitMoveData data in units)
            {
                if (data.finished || data.path.Count == 0)
                    continue;

                anyMoving = true;

                Vector2Int next = data.path.Dequeue();
                movingThisStep++;

                data.unit.MoveOneStep(next, () =>
                {
                    movingThisStep--;
                });
            }

            // Esperar a que TODAS las unidades terminen su paso
            while (movingThisStep > 0)
                yield return null;
        }

        EndExecution();
    }

    private void EndExecution()
    {
        IsRunning = false;
        units.Clear();

        if (TurnManager.Instance != null)
            TurnManager.Instance.EndMovementExecution();

        foreach (Unit u in FindObjectsOfType<Unit>())
        {

            u.ResetPushState();
            u.ClearPlannedMovement();
        }
    }
}
