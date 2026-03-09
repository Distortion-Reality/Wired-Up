using Fusion;

[System.Serializable]
public struct LevelSpawnInfo
{
    [Networked] public CharacterColor Center { get; set; }
    [Networked] public CharacterColor Left { get; set; }
    [Networked] public CharacterColor Right { get; set; }
}
