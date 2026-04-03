using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LevelSpawnInfo;

public class LobbyManager : SimulationBehaviour
{
    private const int MAX_PLAYERS = 3;

    public struct LobbyPlayerInfo
    {
        public PlayerRef playerRef;
        public LobbyPlayer lobbyPlayer;

        public bool IsValid => playerRef.IsValid && lobbyPlayer != null;

        public void Set(LobbyPlayer player) => (playerRef, lobbyPlayer) = (player.Object.InputAuthority, player);

        public void Reset() => (playerRef, lobbyPlayer) = (PlayerRef.None, null);
    }

    private static LobbyPlayerInfo left, center, right;

    Transform canvas;

    public NetworkPrefabRef lobbyPlayerPrefab;
    public NetworkPrefabRef levelSpawnInfoPrefab;
    public float xSpawnPosOffset = 250.0f;
    public bool forceStart = false;

    private Dictionary<PlayerRef, LobbyPlayer> allPlayers =
        new Dictionary<PlayerRef, LobbyPlayer>();
    public List<LobbyPlayer> Players => new List<LobbyPlayer>(allPlayers.Values);

    public static LobbyManager Instance { get; private set; }

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

        player.transform.localPosition += FindAvailableSpawnPosition(player);

        allPlayers[player.Object.InputAuthority] = player;
    }

    public void UnregisterPlayer(PlayerRef player)
    {
        if (player == center.playerRef)
            center.Reset();
        else if (player == left.playerRef)
            left.Reset();
        else if (player == right.playerRef)
            right.Reset();

        allPlayers.Remove(player);

        allPlayers = allPlayers
            .Where(player => player.Value != null)
            .ToDictionary(player => player.Key, player => player.Value);
    }

    Vector3 FindAvailableSpawnPosition(LobbyPlayer player)
    {
        float x = 0f;

        // Owner is always the center
        if (!player.Object.HasInputAuthority)
        {
            List<LobbyPlayer> otherPlayers = Players
                .Where(player => player != null && !player.Object.HasInputAuthority)
                .ToList();

            if (player.Object.InputAuthority == right.playerRef)
            {
                // Choose right
                x = xSpawnPosOffset;
            }
            else if (player.Object.InputAuthority == left.playerRef || otherPlayers.Count == 0)
            {
                // Choose left
                x = -xSpawnPosOffset;
            }
            else
            {
                // Choose opposite of the occupied slot
                x = -otherPlayers[0].transform.localPosition.x;
            }
        }

        if (x < 0)
        {
            left.Set(player);
        }
        else if (x == 0)
        {
            center.Set(player);
        }
        else if (x > 0)
        {
            right.Set(player);
        }

        return new Vector3(x, 0f, 0f);
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
        return player.Object.HasStateAuthority &&
            player.Object.HasInputAuthority &&
            forceStart &&
            player.IsReady;
    }

    private bool CanStart()
    {
        return Players.Count == 3 && Players.TrueForAll(player => player.IsReady);
    }

    public void StartGame(LobbyPlayer player)
    {
        if (Runner.IsServer && (CanStart() || ForceStart(player)))
        {
            if (LevelSpawnInfo.Instance == null)
                Runner.Spawn(levelSpawnInfoPrefab);

            if (LevelSpawnInfo.Instance.Object.HasStateAuthority)
            {
                if (center.IsValid)
                    LevelSpawnInfo.Instance.Center = new SpawnInfo(center);

                if (left.IsValid)
                    LevelSpawnInfo.Instance.Left = new SpawnInfo(left);

                if (right.IsValid)
                    LevelSpawnInfo.Instance.Right = new SpawnInfo(right);
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
        if (!runner.IsServer)
            return;

        if (LevelSpawnInfo.Instance)
        {
            if (LevelSpawnInfo.Instance.Center.player.IsValid)
            {
                SpawnPlayer(runner, LevelSpawnInfo.Instance.Center.player);
            }
            if (LevelSpawnInfo.Instance.Left.player.IsValid)
            {
                SpawnPlayer(runner, LevelSpawnInfo.Instance.Left.player);
            }
            if (LevelSpawnInfo.Instance.Right.player.IsValid)
            {
                SpawnPlayer(runner, LevelSpawnInfo.Instance.Right.player);
            }

            runner.Despawn(LevelSpawnInfo.Instance.Object);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
