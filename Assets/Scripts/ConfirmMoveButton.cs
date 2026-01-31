using UnityEngine;

public class ConfirmMoveButton : MonoBehaviour
{
    public void OnClickMove()
    {
        Debug.Log("BOTÓN MOVE PULSADO - iniciar ejecución de movimiento");
        TurnManager.Instance.StartMovementExecution();
    }
}
