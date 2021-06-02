using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Fighter
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // Statistics initialization

        FighterStatistic hp = new FighterStatistic(20);
        FighterStatistic arm = new FighterStatistic(10);
        FighterStatistic spd = new FighterStatistic(10);
        FighterStatistic lng = new FighterStatistic(10);
        FighterStatistic eng = new FighterStatistic(20);
        FighterStatistic intensity = new FighterStatistic(10);

        stats = new Dictionary<StatisticManager.StatisticId, FighterStatistic>()
        {
            { StatisticManager.StatisticId.HP, hp },
            { StatisticManager.StatisticId.Arm, arm },
            { StatisticManager.StatisticId.Spd, spd },
            { StatisticManager.StatisticId.Lng, lng },
            { StatisticManager.StatisticId.Eng, eng },
            { StatisticManager.StatisticId.Int, intensity }
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
        if (CheckEnergy(ability))
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
