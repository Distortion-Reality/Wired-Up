using Photon.Bolt;

public class EnemySpawner : EntitySpawner
{
    public EnemyId enemyId;

    protected override FighterInfo CreateToken()
    {
        return new EnemyInfo();
    }

    protected override FighterInfo BuildToken()
    {
        EnemyInfo token = (EnemyInfo) base.BuildToken();
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
