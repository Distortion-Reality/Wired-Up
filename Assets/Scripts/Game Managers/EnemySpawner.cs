using UnityEngine;
using Photon.Bolt;

public class EnemySpawner : EntitySpawner
{
    public GameObject modelPrefab;

    protected override BoltEntity InstantiatePrefab(FighterInfo info)
    {
        BoltEntity boltEntity = base.InstantiatePrefab(info);
        Instantiate(modelPrefab, parent: boltEntity.transform);
        return boltEntity;
    }
}
