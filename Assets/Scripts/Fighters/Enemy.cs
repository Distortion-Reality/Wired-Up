using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

[RequireComponent(typeof(TargetEnemyAbilityManager))]
public class Enemy : Fighter
{
    public GameObject enemyHealthBarPrefab;
    const float HealthBarMinDistance = 10f;
    const float HealthBarMaxDistance = 60f;

    protected new IEnemyState State => entity.GetState<IEnemyState>();
    
    protected override void InitStats()
    {
        FighterRangedStatistic hp = new FighterRangedStatistic(20);
        FighterBuffableStatistic armor = new FighterBuffableStatistic(50);
        FighterBuffableStatistic length = new FighterBuffableStatistic(70);
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

        healthBar = Instantiate(enemyHealthBarPrefab, parent: gui.transform).GetComponent<Slider>();
    }

    public override void EntityUpdate()
    {
        base.EntityUpdate();

        UpdateHealthBarTransform();
    }

    protected virtual void UpdateHealthBarTransform()
    {
        float cameraDistance = Vector3.Distance(Camera.main.transform.position, transform.position);
        if (cameraDistance > HealthBarMaxDistance)
            healthBar.gameObject.SetActive(false);
        else
        {
            healthBar.transform.position  = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 2f, 0f));
            Vector3 scale = Vector3.one * (HealthBarMinDistance / cameraDistance);
            healthBar.transform.localScale = scale;
            healthBar.gameObject.SetActive(true);
        }
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
