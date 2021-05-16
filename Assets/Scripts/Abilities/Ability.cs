using System.Collections;
using System.Collections.Generic;

public class Ability
{
    List<Effect> effects;
    int lng;

    public Ability(List<Effect> effects, int lng)
    {
        this.effects = effects;
        this.lng = lng;
    }

    public int Lng { get => lng; }

    public void DoAbility(Fighter user)
    {
        foreach(Effect effect in effects)
            effect.ApplyEffect(user);

        user.EndAbility();
    }
}
