using Photon.Bolt;

public abstract class Ability
{
    public abstract int Energy { get; }

    public abstract void DoAbility(Fighter user);

    protected void EndAbility(Fighter user)
    {
        if (user.Entity.IsOwner)
            user.EndAbility();
        else
            EndAbilityEvent.Post(user.Entity.Source, ReliabilityModes.ReliableOrdered, user.EntityId);
    }
}
