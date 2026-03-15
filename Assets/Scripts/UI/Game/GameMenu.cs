using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public GameObject panel;
    NetworkRunner runner;

    public bool IsOpen => panel.activeSelf;

    void Start()
    {
        runner = NetworkRunnerManager.Instance.Runner;
    }

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
