using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    readonly string bossName = "Kirin";

    public override ParticlesId DamageParticles => ParticlesId.KirinDamage;
    public override Vector3 FirePosition =>
        new Vector3(base.FirePosition.x, target.CentrePosition.y, base.FirePosition.z);

    protected override void InitStats()
    {
        FighterRangedStatistic hp = new FighterRangedStatistic(500);
        FighterBuffableStatistic armor = new FighterBuffableStatistic(100);
        FighterBuffableStatistic length = new FighterBuffableStatistic(50);
        FighterBuffableStatistic intensity = new FighterBuffableStatistic(100);
        FighterEnergy energy = new FighterEnergy(100);
        FighterBuffableStatistic speed = new FighterBuffableStatistic(75);

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

        healthBar.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = bossName;
    }

    protected override void UpdateHealthBarTransform()
    {
        // Health bar is fixed on canvas
    }
}
