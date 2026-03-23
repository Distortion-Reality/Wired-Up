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
        token.data.enemyId = enemyId;
        return token; 
    }

    protected override NetworkObject SpawnEntity()
    {
        NetworkObject entity = base.SpawnEntity();
        GetComponentInParent<EnemyGroup>().AddEnemy(entity.GetComponent<Enemy>());
        return entity;
    }
}
