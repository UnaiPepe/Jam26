using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneButton : MonoBehaviour
{
    public void ChangeSceneToNum(int sceneToChange)
    {

        switch (sceneToChange)
        {
            case 0:
                SceneManager.LoadScene("0_Disclaimer");
                break;
            case 1:
                SceneManager.LoadScene("1_PvE");
                break;
            case 2:
                SceneManager.LoadScene("2_PvP");
                break;
            case 3:
                SceneManager.LoadScene("3_PvPvE");
                break;
        }
    }

}
