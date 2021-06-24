using UnityEngine;

public class Healing : Effect
{
    readonly float percentage;

    public Healing(float percentage)
    {
        this.percentage = percentage;
    }

    protected override void ApplySingleEffect(Fighter user, Fighter target)
    {
        target.ApplyStatChange(StatisticManager.StatisticId.HP,
            Mathf.RoundToInt(target.Stats[StatisticManager.StatisticId.HP].BaseValue * percentage));
    }
}
