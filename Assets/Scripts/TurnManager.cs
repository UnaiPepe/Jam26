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

    // Called by the Move button
    public void StartMovementExecution()
    {
        if (CurrentState != TurnState.Planning)
            return;

        CurrentState = TurnState.MovementExecution;
        MovementExecution.Instance.Begin();
    }

    // Called by MovementExecution
    public void EndMovementExecution()
    {
        CurrentState = TurnState.ActionPhase;
        Debug.Log("Movement finished. Action phase pending.");
    }

    // Used by MovementPreview to block input
    public bool IsExecutingMovement()
    {
        return CurrentState == TurnState.MovementExecution;
    }
}
