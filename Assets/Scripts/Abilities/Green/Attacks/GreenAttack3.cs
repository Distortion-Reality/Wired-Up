public class GreenAttack3 : Ability
{
    const int power = 20;

    public override int Energy => 30;
    public override AbilityId Id => AbilityId.GreenAttack3;

    public override void DoAbility(Fighter user, Fighter target)
    {
        Effects.CalculateAndApplyDamage(user, target, power);
        Effects.Stun(user, target);

        user.EndAbility();
    }
}
