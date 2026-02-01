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
    public GameObject VISUALS;
    public float cinematicDuration = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void PutScene(int id1, int id2)
    {
        defensorPosReference= defensorPos.transform.position;
        atacantePosReference= atacantePos.transform.position; 
        defensor = GameObject.Instantiate (luchadores[id1],defensorPosReference, Quaternion.identity);
        atacante = GameObject.Instantiate (luchadores[id2],atacantePosReference,Quaternion.identity);
        defensorAnim = defensor.GetComponent<Animator>();
        atacanteAnim = atacante.GetComponent<Animator>();
        // Set scale: defensor (1,1,1), atacante (1,-1,1)
        defensor.transform.localScale = new Vector3(1f, 1f, 1f);
        atacante.transform.localScale = new Vector3(1f, -1f, 1f);

    }

    public void Cinematic(int idatack, int iddefence)
    {
        if (VISUALS != null) VISUALS.SetActive(true);
        Debug.Log($"Cinematic called with idattack: {idatack}, iddefence: {iddefence}");
        int a = idatack;
        int b = iddefence;
        PutScene(b,a);
        StartCoroutine("AccionCinematic");

    }
    IEnumerator AccionCinematic()
    {
        yield return new WaitForSeconds(1f); // espera 2 segundos
        StartCoroutine("MorirCO");
    }
    IEnumerator MorirCO()
    {
        atacanteAnim.SetTrigger("Atacar");
        yield return new WaitForSeconds(1f); // espera 2 segundos
        defensorAnim.SetTrigger("RecibirDano");
        //Animar para que salga de pantalla
        yield return new WaitForSeconds(cinematicDuration);
        if (VISUALS != null) VISUALS.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
