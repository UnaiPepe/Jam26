using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
        private Unit unit;
        private bool plannedThisPlanning = false;

        void Awake()
        {
            unit = GetComponent<Unit>();
        }

        void Update()
        {
            if (TurnManager.Instance == null) return;

            // Solo planifica durante Planning, una vez
            if (TurnManager.Instance.CurrentState == TurnState.Planning)
            {
                if (!plannedThisPlanning)
                {
                    PlanAndTelegraph();
                    plannedThisPlanning = true;
                }
            }
            else
            {
                // al salir de planning, resetea para el siguiente turno
                plannedThisPlanning = false;
            }
        }

        void PlanAndTelegraph()
        {
            // Encuentra el jugador más cercano (Unit sin EnemyAI)
            Unit target = FindNearestPlayerUnit();
            if (target == null)
            {
                unit.ClearPlannedMovement();
                MovementPreview.Instance?.AI_HideGhostAndArrow(unit);
                return;
            }

            // Path hacia el jugador
            List<Vector2Int> path = Pathfinder.FindPath(unit.GridPosition, target.GridPosition);
            if (path == null || path.Count == 0)
            {
                unit.ClearPlannedMovement();
                MovementPreview.Instance?.AI_HideGhostAndArrow(unit);
                return;
            }

            // Elige destino avanzando por el path hasta movementRange
            int maxSteps = Mathf.Max(0, unit.movementRange);
            int index = Mathf.Min(maxSteps, path.Count - 1);

            Vector2Int destination = path[index];

            // Si por lo que sea no se mueve
            if (destination == unit.GridPosition)
            {
                unit.ClearPlannedMovement();
                MovementPreview.Instance?.AI_HideGhostAndArrow(unit);
                return;
            }

            // Planifica
            unit.SetPlannedDestination(destination);

            // Muestra ghost + flecha (lo que hará)
            MovementPreview.Instance?.AI_ShowGhostAndArrow(unit, destination);

            Debug.Log("[EnemyAI] {name} planea moverse a {destination} (objetivo: {target.name})");
        }

        Unit FindNearestPlayerUnit()
        {
            Unit[] all = FindObjectsOfType<Unit>();

            Unit best = null;
            int bestDist = int.MaxValue;

            foreach (Unit u in all)
            {
                if (u == unit) continue;

                // Jugador = Unit que NO tenga EnemyAI
                if (u.GetComponent<Enemy>() != null) continue;

                int dist = Mathf.Abs(u.GridPosition.x - unit.GridPosition.x) +
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
