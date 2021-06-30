using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

public class Boss : Enemy
{
    readonly string bossName = "Kirin";
    const float HealthBarMaxDistance = 60f;

    public override ParticlesId DamageParticles => ParticlesId.KirinDamage;
    public override Vector3 FirePosition =>
        new Vector3(base.FirePosition.x, target.CentrePosition.y, base.FirePosition.z);

    protected override void InitStats()
    {
        FighterRangedStatistic hp = new FighterRangedStatistic(200);
        FighterBuffableStatistic armor = new FighterBuffableStatistic(100);
        FighterBuffableStatistic length = new FighterBuffableStatistic(100);
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

    protected override void UpdateHealthBar()
    {
        if (!healthBar.gameObject.activeSelf)
        {
            Collider[] colliders = new Collider[3];
            Physics.OverlapSphereNonAlloc(BottomPosition, HealthBarMaxDistance, colliders,
                LayerMask.GetMask("Player"));
            healthBar.gameObject.SetActive(true);
        }
    }

    protected override void Die()
    {
        base.Die();

        GameWinEvent.Post(ReliabilityModes.ReliableOrdered);
    }

    protected override void UseAbility(Ability ability)
    {
        FighterStatus = Status.Using;
        animator.SetTrigger("AttackShoot");

        StartCoroutine(WaitForAnimation(ability));
    }

    IEnumerator WaitForAnimation(Ability ability)
    {
        yield return new WaitForSeconds(1f);
        base.UseAbility(ability);
    }
}
