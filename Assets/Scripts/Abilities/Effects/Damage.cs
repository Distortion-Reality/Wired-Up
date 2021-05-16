using System.Collections;
using System.Collections.Generic;

public class Damage : Effect
{
    int power;

    public Damage(int power)
    {
        this.power = power;
    }

    public override void ApplyEffect(Fighter user)
    {
        user.Target.Stats[StatisticManager.StatisticId.HP].CurrentValue -= power;
    }
}
