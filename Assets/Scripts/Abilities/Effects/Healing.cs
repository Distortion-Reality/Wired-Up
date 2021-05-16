using System.Collections;
using System.Collections.Generic;

public class Healing : Effect
{
    int amount;

    public Healing(int amount)
    {
        this.amount = amount;
    }

    public override void ApplyEffect(Fighter user)
    {
        user.Target.Stats[StatisticManager.StatisticId.HP].CurrentValue += amount;
    }
}
