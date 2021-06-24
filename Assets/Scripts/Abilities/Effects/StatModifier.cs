using System.Collections;
using UnityEngine;

public class StatModifier : Effect
{
    readonly StatisticManager.StatisticId statId;
    readonly int stages;
    readonly float time = 45f;

    public StatModifier(StatisticManager.StatisticId statId, int stages, float time)
    {
        this.statId = statId;
        this.stages = stages;
        this.time = time;
    }

    protected override void ApplySingleEffect(Fighter user, Fighter target)
    {
        target.TargetAbilityManager.StartCoroutine(ApplyStatChange(target));
    }

    IEnumerator ApplyStatChange(Fighter target)
    {
        target.ApplyStatChange(statId, stages);

        yield return new WaitForSeconds(time);

        target.ApplyStatChange(statId, - stages);
    }
}
