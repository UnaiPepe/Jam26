using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FaceManager : MonoBehaviour
{
    public static FaceManager Instance { get; private set; }

    [Header("Assets")]
    [SerializeField] private Sprite[] luchadorFaces;

    private List<int> fighterID = new List<int>();

    [Header("UI Components")]
    [SerializeField] private GameObject[] faceButtons;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PutLuchadorFaces()
    {
        fighterID.Clear();

        Unit[] luchadores = FindObjectsOfType<Unit>();

        foreach (Unit luchador in luchadores)
        {
            fighterID.Add(luchador.luchadorID);
        }

        int count = Mathf.Min(faceButtons.Length, fighterID.Count);

        for (int i = 0; i < count; i++)
        {
            int chosenMask = fighterID[i];

            if (chosenMask < 0 || chosenMask >= luchadorFaces.Length)
            {
                Debug.LogWarning($"ID de luchador fuera de rango: {chosenMask}");
                continue;
            }

            Image img = faceButtons[i].GetComponent<Image>();
            img.sprite = luchadorFaces[chosenMask];
        }
    }
}
