using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Fighter
{
    Wire wire;

    public Player(Dictionary<StatisticManager.StatisticId, FighterStatistic> stats,
        List<Ability> abilities, List<Ability> assists, TargetPlayerAbilityManager targetPlayerAbilityManager, Wire wire) :
        base(stats, abilities, assists, targetPlayerAbilityManager)
    {
        this.wire = wire;
    }

    protected override void PrepareToUseAbility(Ability ability)
    {
        CheckFighterAbilityStatus();

        // Instantiate(WireParent come figlio di this )
        fighterAbilityStatus = AbilityStatus.EXTENDING;
        wire.Connect(ability);
    }

    public override void UseAbility(Ability ability)
    {
        fighterAbilityStatus = AbilityStatus.WAITING;
        this.target.TargetAbilityManager.EnqueueUserAbility(this, ability);
    }

    public override void InterruptAbility()
    {
        target.TargetAbilityManager.DequeueUserAbility(this);
        base.InterruptAbility();
    }

    public override void EndAbility()
    {
        // GameObject.destroy();
        base.EndAbility();
    }
}
