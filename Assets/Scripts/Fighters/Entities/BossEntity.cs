using Fusion;
using UnityEngine;

[RequireComponent(typeof(Boss))]
public class BossEntity : EnemyEntity
{
    [Networked] public float Speed { get; set; }
    [Networked] public float Direction { get; set; }
    [Networked] public bool OnGround { get; set; }

    Boss Boss => Enemy as Boss;

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_AttackShoot()
    {
        Boss.AttackShoot();
    }
}
