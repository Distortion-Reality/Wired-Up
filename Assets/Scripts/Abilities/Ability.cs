using System.Collections.Generic;

public class Ability
{
    readonly List<Effect> effects;
    readonly int energy;
    readonly string trigger;

    public Ability(List<Effect> effects, int energy, string trigger)
    {
        this.effects = effects;
        this.energy = energy;
        this.trigger = trigger;
    }

    public int Energy => energy;

    public void DoAbility(Fighter user)
    {
        user.Animator.SetTrigger(trigger);
    }

    public void ApplyEffects(Fighter user)
    {
        foreach (Effect effect in effects)
            effect.ApplyEffect(user);
    }
}
