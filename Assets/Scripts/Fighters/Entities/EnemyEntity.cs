using Fusion;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(NetworkTransform))]
public class EnemyEntity : FighterEntity
{
    [Networked] public EnemyId EnemyId { get; private set; }

    public Enemy Enemy => fighter as Enemy;
    public EnemyToken EnemyToken { get => Token as EnemyToken; set => Token = value; }

    public NetworkPrefabRef enemyAttackRayPrefab;

    public override void UnwrapAttachedToken()
    {
        base.UnwrapAttachedToken();

        if (Object.HasStateAuthority)
        {
            EnemyId = EnemyToken.data.enemyId;
        }
    }
}
