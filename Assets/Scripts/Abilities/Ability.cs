public abstract class Ability
{
    public abstract int Energy { get; }

    public abstract void DoAbility(Fighter user);

    protected void EndAbility(Fighter user)
    {
        user.EndAbility();
    }
}
