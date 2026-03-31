using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BossEntity))]
public class Boss : Enemy
{
    public BossEntity BossEntity => EnemyEntity as BossEntity;

    float Direction { get => BossEntity.Direction; set => BossEntity.Direction = value; }
    bool OnGround { get => BossEntity.OnGround; set => BossEntity.OnGround = value; }

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

        animator.fireEvents = false;

        AudioManager.Instance.enabled = true;
    }

    protected override void UpdateHealthBar()
    {
        if (!healthBar.gameObject.activeSelf)
        {
            Collider[] colliders = new Collider[3];
            Physics.OverlapSphereNonAlloc(BottomPosition, HealthBarMaxDistance, colliders,
                LayerMask.GetMask("Player"));
            foreach (Collider collider in colliders)
                if (collider)
                    healthBar.gameObject.SetActive(true);
        }
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();

        Direction = 0f;
        OnGround = true;
    }

    protected override void Die()
    {
        NetworkManager.RPC_GameWin(BossEntity.Runner);

        base.Die();
    }

    protected override void UseAbility(Ability ability)
    {
        FighterStatus = Status.Using;
        BossEntity.RPC_AttackShoot();

        StartCoroutine(WaitForAnimation(ability));
    }

    public void DirectionChanged()
    {
        animator.SetFloat("Direction", Direction);
    }

    public void OnGroundChanged()
    {
        animator.SetBool("OnGround", OnGround);
    }

    public void AttackShoot()
    {
        animator.SetTrigger("AttackShoot");
    }

    IEnumerator WaitForAnimation(Ability ability)
    {
        yield return new WaitForSeconds(1f);

        Effects.SpawnParticles(this, ParticlesId.KirinBurst, CharacterUnityColor);
        base.UseAbility(ability);
    }
}
