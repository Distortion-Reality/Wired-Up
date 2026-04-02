using Fusion;
using UnityEngine;

[RequireComponent(typeof(Boss))]
public class BossEntity : EnemyEntity
{
    [Networked(OnChanged = nameof(OnDirectionChanged)), UnityRange(-1f, 0f)]
    public float Direction { get; set; } = 0f;
    [Networked(OnChanged = nameof(OnGroundChanged))]
    public bool OnGround { get; set; } = false;

    public Boss Boss => Enemy as Boss;

    static void OnDirectionChanged(Changed<BossEntity> changed)
    {
        changed.Behaviour.Boss.DirectionChanged();
    }

    static void OnGroundChanged(Changed<BossEntity> changed)
    {
        changed.Behaviour.Boss.OnGroundChanged();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_AttackShoot()
    {
        Boss.AttackShoot();
    }
}
