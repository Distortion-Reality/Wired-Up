using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private const int MAX_PLAYERS = 3;

    public static LobbyManager Instance { get; private set; }

    NetworkRunner runner;
    Transform canvas;

    public NetworkObject lobbyPlayerPrefab;
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
        runner = NetworkRunnerManager.Instance.Runner;

        runner.AddCallbacks(this);
        canvas = GetComponent<Canvas>().transform;
    }

    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        // Spawn lobby player
        // Force reset position and rotation because they start with weird values (?)
        runner.Spawn(lobbyPlayerPrefab, Vector3.zero, Quaternion.identity, player);
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

    public void Shutdown(NetworkRunner runner)
    {
        runner.Shutdown();
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void CheckOwnerCharacterAvailable(NetworkRunner runner)
    {
        if (allPlayers.TryGetValue(runner.LocalPlayer, out LobbyPlayer owner))
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
        return player.Object.HasInputAuthority && forceStart;
    }

    private bool CanStart()
    {
        return Players.Count == 3 && Players.TrueForAll(player => player.IsReady);
    }

    public void StartGame(NetworkRunner runner, LobbyPlayer player)
    {
        if (runner.IsServer && (CanStart() || ForceStart(player)))
        {
            runner.Spawn(levelSpawnInfoPrefab);

            if (!forceStart && player.Object.HasStateAuthority)
            {
                LevelSpawnInfo.Instance.Center = (CharacterColor)Players[0].CurrentCharacterIndex;
                LevelSpawnInfo.Instance.Left = (CharacterColor)Players[1].CurrentCharacterIndex;
                LevelSpawnInfo.Instance.Right = (CharacterColor)Players[2].CurrentCharacterIndex;
            }

            runner.SetActiveScene("Level2Scene");
        }
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
        CheckOwnerCharacterAvailable(runner);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        ReturnToMenu();
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }
}
