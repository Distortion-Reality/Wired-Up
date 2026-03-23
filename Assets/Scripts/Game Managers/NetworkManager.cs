using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PlayerEntity;

public class NetworkManager : SimulationBehaviour, INetworkRunnerCallbacks
{
    public NetworkPrefabRef playerPrefab;
    GameOver gameOver;

    readonly Dictionary<PlayerRef, PlayerEntity> players = new Dictionary<PlayerRef, PlayerEntity>();
    int allyCount = -1;

    public int AllyCount { get => allyCount; }
    public ParticlesManager ParticlesManager { get; private set; }

    public static NetworkManager Instance { get; private set; }

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
        ParticlesManager = GetComponent<ParticlesManager>();
        gameOver = FindObjectOfType<GameOver>();

        Runner = NetworkRunnerManager.Instance.Runner;
        Runner.AddCallbacks(this);
    }

    public void RegisterPlayer(PlayerRef playerRef, PlayerEntity player)
    {
        players.Add(playerRef, player);
        allyCount++;
    }

    public void UnregisterPlayer(PlayerRef playerRef)
    {
        players.Remove(playerRef);
    }

    public void GameLose(string playerName, CharacterColor character)
    {
        gameOver.Lose(playerName, character);
    }

    public void GameWin()
    {
        gameOver.Win();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public static void RPC_SpawnPlayer(NetworkRunner runner, NetworkPrefabRef playerPrefab,
        PlayerRef player, PlayerToken.PlayerData tokenData)
    {
        if (!runner.IsServer)
            return;

        PlayerToken token = new PlayerToken()
        {
            data = tokenData
        };

        LevelSpawnInfo spawnInfo = LevelSpawnInfo.Instance;
        Transform spawnPoint = GameObject.Find("PlayersSpawnPoint").transform;
        Vector3 spawnPosition = spawnPoint.position;
        if (player != runner.LocalPlayer)
        {
            if (tokenData.character == spawnInfo.Left)
                spawnPosition += Vector3.left * 5;
            else if (tokenData.character == spawnInfo.Right)
                spawnPosition += Vector3.right * 5;
        }

        runner.Spawn(
            playerPrefab,
            spawnPosition,
            spawnPoint.rotation,
            player,
            (runner, obj) =>
            {
                PlayerEntity playerEntity = obj.GetComponent<PlayerEntity>();
                playerEntity.PlayerToken = token;
            }
        );
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        runner.Shutdown();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (players.TryGetValue(runner.LocalPlayer, out PlayerEntity playerEntity))
        {
            NetworkInputData data = new NetworkInputData();
            data.dir = playerEntity.Player.GetMovementInput();
            data.rotation = playerEntity.Player.GetRotationInput();
            input.Set(data);
        }
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner)
    {
        runner.Shutdown();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Destroy(GameObject.Find("Menu Audio"));

        // Spawn player
        CharacterManager characterManager = FindObjectOfType<CharacterManager>();
        PlayerToken token = new PlayerToken();
        token.data.name = PlayerPrefs.GetString(PlayerPrefKey.PlayerName);
        token.data.character = characterManager.CurrentCharacter;

        RPC_SpawnPlayer(runner, playerPrefab,
            runner.LocalPlayer, token.data);

        if (!runner.IsServer)
            return;

        foreach (EntitySpawner spawner in FindObjectsOfType<EntitySpawner>())
        {
            spawner.StartEntitySpawner();
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner) { }
}
