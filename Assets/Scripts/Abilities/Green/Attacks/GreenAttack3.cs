public class GreenAttack3 : Ability
{
    const int power = 20;

    public override int Energy => 30;

    public override void DoAbility(Fighter user)
    {
        Effects.ApplyDamage(user, user.Target, power);
        user.Target.ApplyStatus(Fighter.Status.Stunned);

        EndAbility(user);
    }
}
