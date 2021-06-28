using UnityEngine;

public class BlueAssist2 : Ability
{
    const int stages = 1;

    public override int Energy => 25;
    public override AbilityId Id => AbilityId.BlueAssist2;

    public override void DoAbility(Fighter user, Fighter target)
    {
        int statIndex = Random.Range(0, StatisticManager.BuffableStats.Count);
        StatisticManager.StatisticId statId = StatisticManager.BuffableStats[statIndex];
        Effects.BuffStat(target, statId, stages);

        user.EndAbility();
    }
}
