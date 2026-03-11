using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public GameObject panel;
    readonly NetworkRunner runner = NetworkRunnerManager.Instance.Runner;

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
        if (runner != null)
        {
            runner.Shutdown();
        }

        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
