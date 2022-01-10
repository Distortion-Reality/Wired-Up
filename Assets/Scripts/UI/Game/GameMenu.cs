using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Bolt;

public class GameMenu : MonoBehaviour
{
    public GameObject panel;

    public bool IsOpen => panel.activeSelf;

    public void Trigger()
    {
        panel.SetActive(!panel.activeSelf);
    }

    public void Resume()
    {
        Trigger();
    }

    public void ReturnToMenu()
    {
        BoltLauncher.Shutdown();
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
