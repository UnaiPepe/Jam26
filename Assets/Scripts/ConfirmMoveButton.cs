using Unity.VisualScripting;
using UnityEngine;

public class ConfirmMoveButton : MonoBehaviour
{
    public void OnConfirmMove()
    {
        MovementPreview.Instance.ConfirmMove();
    }
}
