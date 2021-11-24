public class PurpleAttack2 : Ability
{
    const int power = 5;
    const float rate = 1f;
    const int times = 5;

    public override int Energy => 20;
    public override AbilityId Id => AbilityId.PurpleAttack2;

    public override void DoAbility(Fighter user, Fighter target)
    {
        Effects.DamageOverTime(user, target, power, rate, times);
        DelayEndAbility(user, user.DamageParticles);
    }
}
