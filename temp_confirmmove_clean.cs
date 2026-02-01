using UnityEngine;

public class ConfirmMoveButton : MonoBehaviour
{
    public void OnClickMove()
    {
        Debug.Log("BOT?N MOVE PULSADO - iniciar ejecuci?n de movimiento");
        TurnManager.Instance.StartMovementExecution();
    }
}
