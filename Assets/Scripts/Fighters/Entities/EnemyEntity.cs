using Fusion;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(NetworkTransform))]
public class EnemyEntity : FighterEntity
{
    [Networked] public EnemyId EnemyId { get; private set; }

    protected Enemy Enemy => fighter as Enemy;
    public EnemyToken EnemyToken { get => Token as EnemyToken; set => Token = value; }

    public NetworkPrefabRef enemyAttackRayPrefab;

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_UnwrapAttachedToken(EnemyId enemyId)
    {
        EnemyId = enemyId;
    }

    public override void UnwrapAttachedToken()
    {
        base.UnwrapAttachedToken();

        if (Object.HasInputAuthority)
        {
            RPC_UnwrapAttachedToken(EnemyToken.enemyId);
        }
    }
}
