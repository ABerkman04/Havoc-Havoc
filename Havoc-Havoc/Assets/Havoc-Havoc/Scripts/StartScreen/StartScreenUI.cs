using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScreenUI : MonoBehaviour
{
    public void OnClickPlay()
    {
        SceneManager.LoadScene(1);
    }

}