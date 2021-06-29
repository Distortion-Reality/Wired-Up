using UnityEngine;
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
}
