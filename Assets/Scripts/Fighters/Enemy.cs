using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

[RequireComponent(typeof(TargetEnemyAbilityManager))]
public class Enemy : Fighter
{
    new IFighterState State { get => entity.GetState<IEnemyState>(); }

    protected override Quaternion DefaultRotation => transform.rotation;
    
    protected override void InitStats()
    {
        FighterRangedStatistic hp = new FighterRangedStatistic(20);
        FighterStatistic armor = new FighterBuffableStatistic(10);
        FighterStatistic length = new FighterBuffableStatistic(10);
        FighterStatistic intensity = new FighterBuffableStatistic(10);
        FighterEnergy energy = new FighterEnergy(20);
        FighterStatistic speed = new FighterBuffableStatistic(50);

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
        List<Effect> effects1 = new List<Effect>()
        {
            new Damage(10)
        };
        Ability ability1 = new Ability(effects1, 5, "");

        List<Effect> effects2 = new List<Effect>()
        {
            new StatModifier(StatisticManager.StatisticId.Int, 10, 10)
        };
        Ability ability2 = new Ability(effects2, 5, "");

        List<Effect> effects3 = new List<Effect>()
        {
            new Healing(10)
        };
        Ability ability3 = new Ability(effects3, 5, "");

        attacks = new List<Ability>()
        {
            ability1,
            ability2,
            ability3
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
