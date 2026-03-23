using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class LevelSpawnInfo : NetworkBehaviour
{
    public static LevelSpawnInfo Instance { get; private set; }

    [Networked] public CharacterColor Center { get; set; }
    [Networked] public CharacterColor Left { get; set; }
    [Networked] public CharacterColor Right { get; set; }

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
