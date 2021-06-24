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
        target.ChangeHP(Mathf.RoundToInt(target.Stats[StatisticManager.StatisticId.HP].BaseValue * percentage));
    }
}
