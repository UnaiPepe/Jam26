using UnityEngine;

public enum TurnState
{
    Planning,
    MovementExecution,
    ActionPhase
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public TurnState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentState = TurnState.Planning;
    }

    // ================= TURN FLOW =================

    // Called by the Move button
    public void StartMovementExecution()
    {
        if (CurrentState != TurnState.Planning)
            return;

        CurrentState = TurnState.MovementExecution;

        if (MovementExecution.Instance != null)
            MovementExecution.Instance.Begin();
    }

    // Called by MovementExecution when ALL units finished moving
    public void EndMovementExecution()
    {
        // Volvemos a planificación para permitir seleccionar otra vez
        CurrentState = TurnState.Planning;

        Debug.Log("Movement finished. Back to planning.");
    }

    // ================= HELPERS =================

    // Used by MovementPreview to block input
    public bool IsExecutingMovement()
    {
        return CurrentState == TurnState.MovementExecution;
    }
}
