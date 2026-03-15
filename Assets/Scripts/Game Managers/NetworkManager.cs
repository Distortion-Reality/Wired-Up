using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : SimulationBehaviour, INetworkRunnerCallbacks
{
    public NetworkPrefabRef playerPrefab;
    GameOver gameOver;

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

    public void IncrementAllyCount()
    {
        allyCount++;
    }

    public void GameLose(string playerName, CharacterColor character)
    {
        gameOver.Lose(playerName, character);
    }

    public void GameWin()
    {
        gameOver.Win();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        runner.Shutdown();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

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

        if (!runner.IsServer)
            return;

        // Spawn player
        CharacterManager characterManager = FindObjectOfType<CharacterManager>();

        LevelSpawnInfo spawnInfo = LevelSpawnInfo.Instance;
        Transform spawnPoint = GameObject.Find("PlayersSpawnPoint").transform;
        Vector3 spawnPosition = spawnPoint.position;

        foreach (var player in runner.ActivePlayers)
        {
            if (characterManager.CurrentCharacter == spawnInfo.Left)
                spawnPosition += Vector3.left * 5;
            else if (characterManager.CurrentCharacter == spawnInfo.Right)
                spawnPosition += Vector3.right * 5;

            PlayerToken token = new PlayerToken();
            token.name = PlayerPrefs.GetString(PlayerPrefKey.PlayerName);
            token.character = characterManager.CurrentCharacter;

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

        foreach (EntitySpawner spawner in FindObjectsOfType<EntitySpawner>())
        {
            spawner.StartEntitySpawner();
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner) { }
}
