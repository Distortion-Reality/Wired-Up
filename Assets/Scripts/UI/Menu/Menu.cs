using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private const int MAX_PLAYERS = 3;

    public TMPro.TMP_InputField playerName;
    public NetworkRunner runner;

    private NetworkSceneManagerDefault sceneManager;

    void Start()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefKey.PlayerName))
        {
            PlayerPrefs.SetString(PlayerPrefKey.PlayerName, GeneratePlayerName());
        }
        playerName.text = PlayerPrefs.GetString(PlayerPrefKey.PlayerName);

        sceneManager = runner.GetComponent<NetworkSceneManagerDefault>();
    }

    public void PlayerNameEndEdit()
    {
        if (playerName.text.CompareTo(string.Empty) == 0)
        {
            playerName.text = GeneratePlayerName();
        }

        PlayerPrefs.SetString(PlayerPrefKey.PlayerName, playerName.text);
    }

    private string GeneratePlayerName()
    {
        return "Player #" + ((uint)Guid.NewGuid().GetHashCode()).ToString().Substring(0, 4);
    }

    public void Host()
    {
        if (runner.gameObject.activeSelf)
            return;

        runner.gameObject.SetActive(true);
        runner.ProvideInput = true;

        runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = Guid.NewGuid().ToString(),
            Scene = SceneUtility.GetBuildIndexByScenePath("Lobby"),
            SceneManager = sceneManager
        });
    }

    public void Join()
    {
        if (runner.gameObject.activeSelf)
            return;

        runner.gameObject.SetActive(true);
        runner.ProvideInput = true;
        runner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    public void OnSessionListUpdate(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        var session = sessionList.FirstOrDefault(
            s => s.IsValid && s.IsVisible && s.IsOpen &&
            s.PlayerCount < s.MaxPlayers && s.PlayerCount < MAX_PLAYERS);

        if (session != null)
        {
            runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = session.Name,
                SceneManager = sceneManager
            });
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
