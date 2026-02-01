using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Unit unit;

    // Para no planificar m?s de una vez por turno
    private int lastPlannedTurn = -1;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void Update()
    {
        if (unit == null)
            return;

        if (TurnManager.Instance == null)
            return;

        // Solo NPC
        if (unit.team != Unit.Team.NPC)
            return;

        // Solo durante Planning
        if (!TurnManager.Instance.IsPlanning())
            return;

        // Ya planific? en este turno
        if (lastPlannedTurn == TurnManager.Instance.TurnCounter)
            return;

        PlanAndTelegraph();
        lastPlannedTurn = TurnManager.Instance.TurnCounter;
    }

    // ================= AI =================

    private void PlanAndTelegraph()
    {
        Unit target = FindNearestPlayerUnit();

        if (target == null)
        {
            unit.ClearPlannedMovement();
            MovementPreview.Instance?.AI_HideGhostAndArrow(unit);
            return;
        }

        List<Vector2Int> path =
            Pathfinder.FindPath(unit.GridPosition, target.GridPosition);

        if (path == null || path.Count == 0)
        {
            unit.ClearPlannedMovement();
            MovementPreview.Instance?.AI_HideGhostAndArrow(unit);
            return;
        }

        int maxSteps = Mathf.Max(0, unit.movementRange);
        int index = Mathf.Min(maxSteps, path.Count - 1);

        Vector2Int destination = path[index];

        if (destination == unit.GridPosition)
        {
            unit.ClearPlannedMovement();
            MovementPreview.Instance?.AI_HideGhostAndArrow(unit);
            return;
        }

        unit.SetPlannedDestination(destination);
        MovementPreview.Instance?.AI_ShowGhostAndArrow(unit, destination);

        Debug.Log(
            "[EnemyAI] " + unit.name +
            " planea moverse a " + destination +
            " (objetivo: " + target.name + ")"
        );
    }

    private Unit FindNearestPlayerUnit()
    {
        Unit[] all = FindObjectsOfType<Unit>();

        Unit best = null;
        int bestDist = int.MaxValue;

        foreach (Unit u in all)
        {
            if (u == unit)
                continue;

            if (u.team == Unit.Team.NPC)
                continue;

            int dist =
                Mathf.Abs(u.GridPosition.x - unit.GridPosition.x) +
                Mathf.Abs(u.GridPosition.y - unit.GridPosition.y);

            if (dist < bestDist)
            {
                bestDist = dist;
                best = u;
            }
        }

        return best;
    }
}
