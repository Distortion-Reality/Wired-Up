using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

public class BossArenaEntrance : GlobalEventListener
{
    readonly List<Player> players = new List<Player>();

    // Update is called once per frame
    void Update()
    {
        if (players.Count == 3)
        {
            foreach (Player player in players)
                if (player.transform.position.z < transform.position.z)
                    return;
            GetComponent<Collider>().enabled = true;
            enabled = false;
        }
    }

    public override void EntityAttached(BoltEntity entity)
    {
        if (entity.StateIs<IPlayerState>())
            players.Add(entity.GetComponent<Player>());
    }
}
