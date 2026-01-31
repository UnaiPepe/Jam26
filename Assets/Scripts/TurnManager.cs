using NUnit.Framework.Interfaces;
using UnityEngine;

public enum TeamTurn
{
    Jugador1,
    Jugador2,
    NPC
}

public enum TurnState
{
    Planning,
    MovementExecution
}
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;




    public TurnState CurrentState { get; private set; }
    public TeamTurn CurrentTeamTurn { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CurrentState = TurnState.Planning;
        CurrentTeamTurn = TeamTurn.Jugador1;
    }

    // ================= TURN FLOW =================

    /// <summary>
    /// Llamado cuando el jugador confirma el Planning
    /// </summary>
    public void StartMovementExecution()
    {
        if (CurrentState != TurnState.Planning)
            return;

        CurrentState = TurnState.MovementExecution;

        if (MovementExecution.Instance != null)
            MovementExecution.Instance.Begin();
        else
            Debug.LogWarning("MovementExecution.Instance es null");
    }

    /// <summary>
    /// Llamado por MovementExecution cuando TODAS las unidades han terminado
    /// </summary>
    public void EndMovementExecution()
    {
        AdvanceTurn();
    }

    // ================= TURN LOGIC =================

    private void AdvanceTurn()
    {
        // Resetear estados temporales de unidades
        ResetUnitsTurnState();

        // Cambiar de equipo
        switch (CurrentTeamTurn)
        {
            case TeamTurn.Jugador1:
                CurrentTeamTurn = TeamTurn.Jugador2;
                break;

            case TeamTurn.Jugador2:
                CurrentTeamTurn = TeamTurn.NPC;
                break;

            case TeamTurn.NPC:
                CurrentTeamTurn = TeamTurn.Jugador1;
                break;
        }

        CurrentState = TurnState.Planning;

        Debug.Log("Turno de: " + CurrentTeamTurn);
    }

    // ================= HELPERS =================

    public bool IsPlanning()
    {
        return CurrentState == TurnState.Planning;
    }

    public bool IsExecutingMovement()
    {
        return CurrentState == TurnState.MovementExecution;
    }

    // ================= INTERNAL =================

    private void ResetUnitsTurnState()
    {
        Unit[] units = FindObjectsOfType<Unit>();

        foreach (Unit u in units)
        {
            u.ResetPushState();
            u.ClearPlannedMovement();
        }
    }
}
