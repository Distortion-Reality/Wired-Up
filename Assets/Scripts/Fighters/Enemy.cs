using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TargetEnemyAbilityManager))]
public class Enemy : Fighter
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // Statistics initialization

        FighterStatistic hp = new FighterStatistic(20);
        FighterStatistic armor = new FighterStatistic(10);
        FighterStatistic length = new FighterStatistic(10);
        FighterStatistic intensity = new FighterStatistic(10);
        FighterStatistic energy = new FighterStatistic(20);
        FighterStatistic speed = new FighterStatistic(10);

        stats = new Dictionary<StatisticManager.StatisticId, FighterStatistic>()
        {
            { StatisticManager.StatisticId.HP, hp },
            { StatisticManager.StatisticId.Arm, armor },
            { StatisticManager.StatisticId.Lng, length },
            { StatisticManager.StatisticId.Int, intensity },
            { StatisticManager.StatisticId.Nrg, energy },
            { StatisticManager.StatisticId.Spd, speed }
        };

        // Abilities initialization

        List<Effect> effects1 = new List<Effect>()
        {
            new Damage(10)
        };
        Ability ability1 = new Ability(effects1, 5);

        List<Effect> effects2 = new List<Effect>()
        {
            new StatModifier(StatisticManager.StatisticId.Int, 10, 10)
        };
        Ability ability2 = new Ability(effects2, 5);

        List<Effect> effects3 = new List<Effect>()
        {
            new Healing(10)
        };
        Ability ability3 = new Ability(effects3, 5);

        attacks = new List<Ability>()
        {
            ability1,
            ability2,
            ability3
        };

        // Assists initialization
        assists = new List<Ability>();

        // TargetAbilityManager initialization
        targetAbilityManager = GetComponent<TargetEnemyAbilityManager>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void UseAbility(Ability ability)
    {
        if (CheckAndUseEnergy(ability.Energy))
        {
            fighterAbilityStatus = AbilityStatus.USING;
            ability.DoAbility(this);
        }
    }

    public override void EndAbility()
    {
        fighterAbilityStatus = AbilityStatus.FREE;
    }
}
