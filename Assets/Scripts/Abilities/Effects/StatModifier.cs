public class StatModifier : Effect
{
    readonly StatisticManager.StatisticId stat;
    readonly int percentage, time;

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
