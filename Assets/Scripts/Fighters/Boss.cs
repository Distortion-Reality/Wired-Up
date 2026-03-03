using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    readonly string bossName = "Kirin";
    const float HealthBarMaxDistance = 60f;
    private float velocity;

    protected new IBossState State => entity.GetState<IBossState>();
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

        State.SetAnimator(animator);
        State.Animator.applyRootMotion = entity.HasStateAuthority;
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

    public override void OwnerFixedUpdate()
    {
        base.OwnerFixedUpdate();

        float speed = Mathf.Abs(agent.velocity.x) + Mathf.Abs(agent.velocity.z);
        speed = Mathf.Clamp(speed, 0f, 0.2f);
        //State.Speed = Mathf.SmoothDamp(State.Speed, speed, ref velocity, 0.1f);
        if (agent.isStopped)
            speed = 0f;
        //State.Speed = speed; owner sees it in wrong position
        State.Direction = 0f;
        State.OnGround = true;
    }

    protected override void Die()
    {
        base.Die();

        GameWinEvent.Post(ReliabilityModes.ReliableOrdered);
    }

    protected override void UseAbility(Ability ability)
    {
        FighterStatus = Status.Using;
        State.AttackShoot();

        StartCoroutine(WaitForAnimation(ability));
    }

    IEnumerator WaitForAnimation(Ability ability)
    {
        yield return new WaitForSeconds(1f);
        base.UseAbility(ability);
    }
}
