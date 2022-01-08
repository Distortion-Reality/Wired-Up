using Photon.Bolt;

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

    protected override BoltEntity InstantiateBoltEntity()
    {
        BoltEntity entity = base.InstantiateBoltEntity();
        GetComponentInParent<EnemyGroup>().AddEnemy(entity.GetComponent<Enemy>());
        return entity;
    }
}
