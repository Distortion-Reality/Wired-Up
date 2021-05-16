using System.Collections;
using System.Collections.Generic;

public class Enemy : Fighter
{
    public Enemy(Dictionary<StatisticManager.StatisticId, FighterStatistic> stats,
        List<Ability> abilities, List<Ability> assists, TargetEnemyAbilityManager targetEnemyAbilityManager) :
        base(stats, abilities, assists, targetEnemyAbilityManager) {}

    protected override void PrepareToUseAbility(Ability ability)
    {
        UseAbility(ability);
    }

    public override void UseAbility(Ability ability)
    {
        fighterAbilityStatus = AbilityStatus.USING;
        ability.DoAbility(this);
    }
}
