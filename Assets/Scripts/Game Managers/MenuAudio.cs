using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuAudio : MonoBehaviour
{
    public static MenuAudio Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SceneManager.activeSceneChanged += ChangedActiveScene;
    }

    void ChangedActiveScene(Scene current, Scene next)
    {
        if (next != SceneManager.GetSceneByName("Menu") &&
            next != SceneManager.GetSceneByName("Lobby"))
        {
            SceneManager.activeSceneChanged -= ChangedActiveScene;
            Destroy(gameObject);
        }
    }
}
