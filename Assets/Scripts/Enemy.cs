using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Unit unit;
    private bool plannedThisPlanning = false;

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

        // Solo NPCs usan este script
        if (unit.team != Unit.Team.NPC)
            return;

        // Solo actúa durante Planning
        if (TurnManager.Instance.CurrentState != TurnState.Planning)
        {
            plannedThisPlanning = false;
            return;
        }

        // Solo planifica en su turno
        if (TurnManager.Instance.CurrentTeamTurn != TeamTurn.NPC)
            return;

        // Planifica una sola vez por turno
        if (plannedThisPlanning)
            return;

        PlanAndTelegraph();
        plannedThisPlanning = true;
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

            // Objetivo = cualquier unidad que NO sea NPC
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
