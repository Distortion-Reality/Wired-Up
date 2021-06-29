public class GreenAttack1 : Ability
{
    const float time = 3f;

    public override int Energy => 10;
    public override AbilityId Id => AbilityId.GreenAttack1;

    public override void DoAbility(Fighter user, Fighter target)
    {
        Effects.BlockMovements(user, target, time);
        user.EndAbility();
    }
}
