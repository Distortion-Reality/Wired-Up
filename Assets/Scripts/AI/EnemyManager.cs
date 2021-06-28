using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

public class EnemyManager : GlobalEventListener
{
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
}
