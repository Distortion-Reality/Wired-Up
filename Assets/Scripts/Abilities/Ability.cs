using Photon.Bolt;

public abstract class Ability
{
    public abstract int Energy { get; }

    public abstract AbilityId Id { get; }

    public abstract void DoAbility(Fighter user, Fighter target);
}
