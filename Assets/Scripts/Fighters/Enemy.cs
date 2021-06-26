using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

[RequireComponent(typeof(TargetEnemyAbilityManager))]
public class Enemy : Fighter
{
    protected new IEnemyState State => entity.GetState<IEnemyState>();
    protected override Quaternion DefaultRotation => transform.rotation;
    
    protected override void InitStats()
    {
        FighterRangedStatistic hp = new FighterRangedStatistic(20);
        FighterBuffableStatistic armor = new FighterBuffableStatistic(50);
        FighterBuffableStatistic length = new FighterBuffableStatistic(10);
        FighterBuffableStatistic intensity = new FighterBuffableStatistic(10);
        FighterEnergy energy = new FighterEnergy(20);
        FighterBuffableStatistic speed = new FighterBuffableStatistic(50);

        stats = new Dictionary<StatisticManager.StatisticId, FighterStatistic>()
        {
            { StatisticManager.StatisticId.HP, hp },
            { StatisticManager.StatisticId.Arm, armor },
            { StatisticManager.StatisticId.Lng, length },
            { StatisticManager.StatisticId.Int, intensity },
            { StatisticManager.StatisticId.Nrg, energy },
            { StatisticManager.StatisticId.Spd, speed }
        };
    }

    public override void EntityStart()
    {
        base.EntityStart();

        // Abilities initialization
        Ability ability1 = new RedAttack1();
        Ability ability2 = new RedAttack1();

        attacks = new List<Ability>()
        {
            ability1,
            ability2,
        };

        // Assists initialization
        assists = new List<Ability>();
    }

    protected override void UseAbility(Ability ability)
    {
        fighterStatus = Status.Using;
        ability.DoAbility(this);
    }

    public override void EndAbility()
    {
        base.EndAbility();
        fighterStatus = Status.Free;
    }
}
