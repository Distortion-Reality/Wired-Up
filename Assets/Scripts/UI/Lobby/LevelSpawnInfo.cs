using Fusion;
using System;
using UnityEngine;
using static LobbyManager;

[RequireComponent(typeof(NetworkObject))]
public class LevelSpawnInfo : NetworkBehaviour
{
    [Serializable]
    public struct SpawnInfo : INetworkStruct
    {
        [HideInInspector] public PlayerRef player;
        public int playerId;
        public CharacterColor character;

        public SpawnInfo(LobbyPlayerInfo playerInfo) => (player, playerId, character) =
            (playerInfo.playerRef,
            playerInfo.playerRef.PlayerId,
            (CharacterColor)playerInfo.lobbyPlayer.CurrentCharacterIndex);
    }

    [Networked] public SpawnInfo Center { get; set; }
    [Networked] public SpawnInfo Left { get; set; }
    [Networked] public SpawnInfo Right { get; set; }

    public static LevelSpawnInfo Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void Spawned()
    {
        if (Instance != null && Instance != this && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
            return;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;
    }
}
