public abstract class Ability
{
    public abstract int Energy { get; }

    public abstract void DoAbility(Fighter user);
}
