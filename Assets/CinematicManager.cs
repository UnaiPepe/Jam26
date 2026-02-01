using System.Collections;
using UnityEngine;

public class CinematicManager : MonoBehaviour
{
    public GameObject[] luchadores; 
    public GameObject defensor, atacante;
    public GameObject defensorPos, atacantePos;
    public Vector3 defensorPosReference, atacantePosReference;
    public int idDefensor, idAtacante;
    public Animator defensorAnim, atacanteAnim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void PutScene()
    {
        defensorPosReference= defensorPos.transform.position;
        atacantePosReference= atacantePos.transform.position; 
        defensor = GameObject.Instantiate (luchadores[idDefensor],defensorPosReference, Quaternion.identity);
        atacante = GameObject.Instantiate (luchadores[idAtacante],atacantePosReference,Quaternion.identity);
        Vector3 escala = defensor.transform.localScale;
        escala.x = Mathf.Abs(escala.x) * -1;
        defensor.transform.localScale = escala;

    }

    public void Cinematic()
    {

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
