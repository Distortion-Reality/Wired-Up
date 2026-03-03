using Fusion;

public class EnemySpawner : EntitySpawner
{
    public EnemyId enemyId;

    protected override FighterToken CreateToken()
    {
        return new EnemyToken();
    }

    protected override FighterToken BuildToken()
    {
        EnemyToken token = (EnemyToken) base.BuildToken();
        token.enemyId = enemyId;
        return token; 
    }

    protected override NetworkObject InstantiateNetworkObject()
    {
        NetworkObject entity = base.InstantiateNetworkObject();
        GetComponentInParent<EnemyGroup>().AddEnemy(entity.GetComponent<Enemy>());
        return entity;
    }
}
