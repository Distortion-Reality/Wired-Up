using Fusion;

public class EnemyEntity : FighterEntity
{
    [Networked] public EnemyId EnemyId { get; private set; }

    protected Enemy Enemy => fighter as Enemy;
    public EnemyToken EnemyToken { get => Token as EnemyToken; set => Token = value; }

    public NetworkPrefabRef enemyAttackRayPrefab;

    public override void UnwrapAttachedToken()
    {
        base.UnwrapAttachedToken();

        EnemyId = EnemyToken.enemyId;
    }
}
