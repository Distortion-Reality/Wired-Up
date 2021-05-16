using System.Collections;
using System.Collections.Generic;

public class StatModifier : Effect
{
    StatisticManager.StatisticId stat;
    int percentage, time;

    public StatModifier(StatisticManager.StatisticId stat, int percentage, int time)
    {
        this.stat = stat;
        this.percentage = percentage;
        this.time = time;
    }

    public override void ApplyEffect(Fighter user)
    {

    }
}
