using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

public class EnemyManager : GlobalEventListener
{
    [Header("Peripherals Prefabs")]
    public GameObject blue;
    public GameObject red;
    public GameObject green;
    public GameObject yellow;
    
    [Header("Bosses Prefabs")]
    public GameObject kirin;

    readonly List<Player> players = new List<Player>();
    readonly List<Enemy> enemies = new List<Enemy>();

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < Mathf.Min(players.Count, enemies.Count); i++)
            if (!enemies[i].Target)
                enemies[i].Target = players[i];
    }

    public override void EntityAttached(BoltEntity entity)
    {
        if (entity.StateIs<IPlayerState>())
            players.Add(entity.GetComponent<Player>());
        else if (entity.StateIs<IEnemyState>())
            enemies.Add(entity.GetComponent<Enemy>());
    }

    public override void EntityDetached(BoltEntity entity)
    {
        if (entity.StateIs<IPlayerState>())
            players.Remove(entity.GetComponent<Player>());
        else if (entity.StateIs<IEnemyState>())
            enemies.Remove(entity.GetComponent<Enemy>());
    }

    public GameObject GetPrefab(EnemyId id)
    {
        switch (id)
        {
            case EnemyId.BluePeripheral:
                return blue;
            case EnemyId.GreenPeripheral:
                return green;
            case EnemyId.RedPeripheral:
                return red;
            case EnemyId.YellowPeripheral:
                return yellow;
            case EnemyId.Kirin:
                return kirin;
        }
        return null;
    }
}
