using Fusion;

public class LevelSpawnInfo : NetworkBehaviour
{
    public static LevelSpawnInfo Instance { get; private set; }

    [Networked] public CharacterColor Center { get; set; }
    [Networked] public CharacterColor Left { get; set; }
    [Networked] public CharacterColor Right { get; set; }

    public override void Spawned()
    {
        if (Instance != null && Instance != this)
        {
            Runner.Despawn(Object);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(Object);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;
    }
}
