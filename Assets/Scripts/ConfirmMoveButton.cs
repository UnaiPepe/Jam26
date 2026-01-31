using UnityEngine;

public class ConfirmMoveButton : MonoBehaviour
{
    public void OnClickMove()
    {
        Debug.Log("BOTÓN MOVE PULSADO");
        MovementPreview.Instance.ConfirmMove();
    }
}
