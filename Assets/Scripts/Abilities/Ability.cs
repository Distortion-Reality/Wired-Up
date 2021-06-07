using System.Collections.Generic;

public class Ability
{
    readonly List<Effect> effects;
    readonly int energy;

    public Ability(List<Effect> effects, int energy)
    {
        this.effects = effects;
        this.energy = energy;
    }

    public int Energy => energy;

    public void DoAbility(Fighter user)
    {
        // user.StartAnimation();
    }

    void OnAnimationEnd(Fighter user)
    {
        foreach (Effect effect in effects)
            effect.ApplyEffect(user);

        user.EndAbility();
    }
}
