public abstract class Ability
{
    public abstract int Energy { get; }

    public virtual AbilityId Id => AbilityId.None;

    public abstract void DoAbility(Fighter user, Fighter target);
}
