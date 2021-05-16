using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : FighterManager
{
    // Start is called before the first frame update
    protected override void Start()
    {
        // Statistics initialization

        FighterStatistic hp = new FighterStatistic(10);
        FighterStatistic intensity = new FighterStatistic(10);
        FighterStatistic arm = new FighterStatistic(10);
        FighterStatistic prc = new FighterStatistic(10);
        FighterStatistic lng = new FighterStatistic(20);
        FighterStatistic spd = new FighterStatistic(10);

        Dictionary<StatisticManager.StatisticId, FighterStatistic> stats =
            new Dictionary<StatisticManager.StatisticId, FighterStatistic>()
            {
                { StatisticManager.StatisticId.HP, hp },
                { StatisticManager.StatisticId.Int, intensity },
                { StatisticManager.StatisticId.Arm, arm },
                { StatisticManager.StatisticId.Prc, prc },
                { StatisticManager.StatisticId.Lng, lng },
                { StatisticManager.StatisticId.Spd, spd }
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

        List<Ability> abilities = new List<Ability>()
        {
            ability1,
            ability2,
            ability3
        };

        // Assists initialization

        List<Ability> assists = new List<Ability>();

        // Wire initialization
        Wire wire = transform.Find("WireParent").transform.Find("Wire").GetComponent<Wire>();

        // TargetAbilityManager initialization
        TargetPlayerAbilityManager targetPlayerAbilityManager = GetComponent<TargetPlayerAbilityManager>();

        // Player initialization
        fighter = new Player(stats, abilities, assists, targetPlayerAbilityManager, wire);

        fighter.Target = GameObject.Find("Enemy1").GetComponent<EnemyManager>().Fighter;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if(Input.GetAxis("Ability1") == 1)
            fighter.TryToUseAbility(fighter.Abilities[0]);
        if(Input.GetAxis("Ability2") == 1)
            fighter.TryToUseAbility(fighter.Abilities[1]);
        if(Input.GetAxis("Ability3") == 1)
            fighter.TryToUseAbility(fighter.Abilities[2]);
    }
}
