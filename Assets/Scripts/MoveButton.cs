using UnityEngine;

public class MoveButton : MonoBehaviour
{
    // Este método se asigna al OnClick() del botón
    public void OnClickMove()
    {
        Debug.Log("BOTÓN MOVE → iniciar ejecución de movimiento");

        TurnManager.Instance.StartMovementExecution();
    }
}
