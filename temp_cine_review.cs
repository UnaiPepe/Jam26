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
    public float cinematicDuration = 4f;

    [Header("Audio")]
public AudioClip[] cinematicAudios;
private AudioSource audioSource;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
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
        defensor.transform.localScale = new Vector3(-1f, 1f, 1f);
        atacante.transform.localScale = new Vector3(1f, 1f, 1f);
        // Set rotation (0, 45, 0) for both
        defensor.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
        atacante.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

    }

    public void Cinematic(int idatack, int iddefence)
    {
        
        if (VISUALS != null) VISUALS.SetActive(true);
        Debug.Log($"Cinematic called with idattack: {idatack}, iddefence: {iddefence}");
        int a = idatack;
        int b = iddefence;

        // Play random audio
    if (cinematicAudios != null && cinematicAudios.Length > 0 && audioSource != null)
    {
        int randomIndex = Random.Range(0, cinematicAudios.Length);
        audioSource.PlayOneShot(cinematicAudios[randomIndex]);
    }

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
        // Destroy instantiated fighters
        if (defensor != null) Destroy(defensor);
        if (atacante != null) Destroy(atacante);
        if (VISUALS != null) VISUALS.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
