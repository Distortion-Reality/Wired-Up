using Fusion;

public class PlayerToken : FighterToken
{
    public struct PlayerData : INetworkStruct
    {
        public FighterData fighterData;
        public NetworkString<_16> name;
        public CharacterColor character;
    }

    public new PlayerData data = new PlayerData();
}
