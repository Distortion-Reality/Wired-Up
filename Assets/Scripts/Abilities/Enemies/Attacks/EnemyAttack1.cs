public class EnemyAttack1 : Ability
{
    const int power = 20;

    public override int Energy => 20;

    public override void DoAbility(Fighter user, Fighter target)
    {
        Effects.CalculateAndApplyDamage(user, target, power);
    }
}
