using Fusion;

public class EnemyToken : FighterToken
{
    public struct EnemyData : INetworkStruct
    {
        public FighterData fighterData;
        public EnemyId enemyId;
    }

    public new EnemyData data = new EnemyData();
}
