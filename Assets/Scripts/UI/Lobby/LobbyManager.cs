using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : SimulationBehaviour
{
    private const int MAX_PLAYERS = 3;

    public static LobbyManager Instance { get; private set; }

    Transform canvas;

    public NetworkPrefabRef lobbyPlayerPrefab;
    public NetworkPrefabRef levelSpawnInfoPrefab;
    public float xSpawnPosOffset = 250.0f;
    public bool forceStart = false;

    private Dictionary<PlayerRef, LobbyPlayer> allPlayers =
        new Dictionary<PlayerRef, LobbyPlayer>();
    public List<LobbyPlayer> Players => new List<LobbyPlayer>(allPlayers.Values);


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        canvas = GetComponent<Canvas>().transform;

        Runner = NetworkRunnerManager.Instance.Runner;
    }

    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        // Spawn lobby player
        // Force reset position and rotation because they start with weird values (?)
        runner.Spawn(
            lobbyPlayerPrefab,
            Vector3.zero,
            Quaternion.identity,
            player);
    }

    private void DespawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (allPlayers.TryGetValue(player, out LobbyPlayer lobbyPlayer))
        {
            runner.Despawn(lobbyPlayer.Object);
        }
    }

    public void RegisterPlayer(LobbyPlayer player)
    {
        player.transform.SetParent(canvas, false);

        if (!player.Object.HasInputAuthority)
        {
            player.transform.localPosition += FindAvailableSpawnPosition();
        }

        allPlayers[player.Object.InputAuthority] = player;
    }

    public void UnregisterPlayer(PlayerRef player)
    {
        allPlayers.Remove(player);

        allPlayers = allPlayers
            .Where(player => player.Value != null)
            .ToDictionary(player => player.Key, player => player.Value);
    }

    Vector3 FindAvailableSpawnPosition()
    {
        float x;

        // Owner is always the center
        List<LobbyPlayer> otherPlayers = Players
            .Where(player => player != null && !player.Object.HasInputAuthority)
            .ToList();

        if (otherPlayers.Count == 0)
        {
            // Choose left
            x = -xSpawnPosOffset;
        }
        else
        {
            // Choose opposite of the occupied slot
            x = -otherPlayers[0].transform.localPosition.x;
        }
        return new Vector3(x, 0.0f, 0.0f);
    }

    public async void Shutdown(NetworkRunner runner)
    {
        await runner.Shutdown();
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void CheckOwnerCharacterAvailable()
    {
        if (allPlayers.TryGetValue(Runner.LocalPlayer, out LobbyPlayer owner))
        {
            List<LobbyPlayer> otherPlayers = Players
                .Where(player => player != null && player != owner)
                .ToList();

            bool available = true;
            for (int i = 0; i < otherPlayers.Count; i++)
            {
                if (otherPlayers[i].IsReady &&
                    otherPlayers[i].CurrentCharacterIndex == owner.CurrentCharacterIndex)
                {
                    available = false;
                }
            }

            owner.ReadyButton.interactable = available;
        }
    }

    private bool ForceStart(LobbyPlayer player)
    {
        return player.Object.HasStateAuthority && forceStart;
    }

    private bool CanStart()
    {
        return Players.Count == 3 && Players.TrueForAll(player => player.IsReady);
    }

    public void StartGame(LobbyPlayer player)
    {
        if (Runner.IsServer && (CanStart() || ForceStart(player)))
        {
            Runner.Spawn(levelSpawnInfoPrefab);

            if (!forceStart && LevelSpawnInfo.Instance.Object.HasStateAuthority)
            {
                LevelSpawnInfo.Instance.Left = (CharacterColor)Players[1].CurrentCharacterIndex;
                LevelSpawnInfo.Instance.Right = (CharacterColor)Players[2].CurrentCharacterIndex;
            }

            Runner.SetActiveScene("Level2Scene");
        }
    }

    public void OnRunnerDisconnectedFromServer(NetworkRunner runner)
    {
        Shutdown(runner);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.ActivePlayers.Count() > MAX_PLAYERS)
        {
            runner.Disconnect(player);
        }
        else if (runner.IsServer)
        {
            SpawnPlayer(runner, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            DespawnPlayer(runner, player);
        }

        UnregisterPlayer(player);
        CheckOwnerCharacterAvailable();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        ReturnToMenu();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer || runner.ActivePlayers.Count() < MAX_PLAYERS)
            return;

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            SpawnPlayer(runner, player);
        }
    }
}
