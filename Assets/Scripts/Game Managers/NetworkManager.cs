using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PlayerEntity;

public class NetworkManager : SimulationBehaviour
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


    void GameLose(string playerName, CharacterColor character)
    {
        gameOver.Lose(playerName, character);
    }

    void GameWin()
    {
        gameOver.Win();
    }

    async void Shutdown(NetworkRunner runner)
    {
        await runner.Shutdown();
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    static void RPC_SpawnPlayer(NetworkRunner runner, NetworkPrefabRef playerPrefab,
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

    [Rpc(RpcSources.All, RpcTargets.All)]
    public static void RPC_SpawnParticle(NetworkRunner runner, NetworkBehaviourId targetId,
        Vector3 position, Quaternion rotation,
        ParticlesId particlesId, Color color)
    {
        bool targetValid = runner.TryFindBehaviour(targetId, out FighterEntity fighterEntity);
        if (targetValid || Instance.ParticlesManager.CanParticlesAfterDeath(particlesId))
        {
            GameObject prefab = Instance.ParticlesManager.GetParticlesPrefab(particlesId);
            GameObject particle = Instantiate(prefab, position, rotation);
            ParticleSystem.MainModule main = particle.GetComponent<ParticleSystem>().main;
            main.startColor = new ParticleSystem.MinMaxGradient(color);

            if (targetValid)
            {
                Fighter target = fighterEntity.fighter;
                particle.transform.SetParent(target.transform, true);
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public static void RPC_GameLose(NetworkRunner runner, string playerName, CharacterColor character)
    {
        Instance.GameLose(playerName, character);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public static void RPC_GameWin(NetworkRunner runner)
    {
        Instance.GameWin();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (gameOver != null && gameOver.IsGameOver)
            return;

        if (players.TryGetValue(runner.LocalPlayer, out PlayerEntity playerEntity) &&
            playerEntity.Object && playerEntity.Object.IsValid)
        {
            NetworkInputData data = new NetworkInputData();
            data.dir = playerEntity.Player.GetMovementInput();
            data.rotation = playerEntity.Player.GetRotationInput();
            input.Set(data);
        }
    }

    public void OnRunnerDisconnectedFromServer(NetworkRunner runner)
    {
        Shutdown(runner);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Shutdown(runner);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        ReturnToMenu();
    }

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
}
