using UnityEngine;

public class Test_Characterclick : MonoBehaviour
{
    public Overlay targeting;

    void OnMouseDown()
    {
        targeting.OpenOverlayFor(gameObject);
    }
}
