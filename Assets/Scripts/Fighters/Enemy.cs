using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TargetEnemyAbilityManager))]
public class Enemy : Fighter
{
    EnemyId id;

    public GameObject enemyHealthBarPrefab;
    const float HealthBarMinDistance = 10f;
    const float HealthBarMaxDistance = 60f;

    protected NavMeshAgent agent;

    protected new IEnemyState State => entity.GetState<IEnemyState>();
    protected override float MovementSpeed => 1.5f * base.MovementSpeed;
    public override float AbilityRange => 1.5f * base.AbilityRange;
    public override ParticlesId DamageParticles => ParticlesId.EnemyDamage;
    public override Color CharacterUnityColor { get => Color.black; }
    public virtual Vector3 FirePosition => CentrePosition;

    protected override void InitStats()
    {
        FighterRangedStatistic hp = new FighterRangedStatistic(100);
        FighterBuffableStatistic armor = new FighterBuffableStatistic(30);
        FighterBuffableStatistic length = new FighterBuffableStatistic(75);
        FighterBuffableStatistic intensity = new FighterBuffableStatistic(30);
        FighterEnergy energy = new FighterEnergy(50);
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

    protected override void UnwrapAttachedToken()
    {
        base.UnwrapAttachedToken();

        EnemyToken info = (EnemyToken) entity.AttachToken;
        id = info.enemyId;
    }

    protected override void LoadModel()
    {
        EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
        GameObject prefab = enemyManager.GetPrefab(id);
        Instantiate(prefab, parent: transform);
    }

    public override void EntityStart()
    {
        base.EntityStart();

        Ability attack1 = new EnemyAttack1();
        Ability attack2 = new EnemyAttack2();

        attacks = new List<Ability>()
        {
            attack1,
            attack2,
        };

        agent = GetComponent<NavMeshAgent>();

        healthBar = Instantiate(enemyHealthBarPrefab, parent: gui.transform).GetComponent<Slider>();

        // Initialize first attack
        currentAbility = attacks[Random.Range(0, attacks.Count)];
    }

    public override void OwnerUpdate()
    {
        base.OwnerUpdate();

        UpdateMovement();

        if (FighterStatus == Status.Free && target)
            UpdateAbility();
    }

    protected override void UpdateMovement()
    {
        if (target &&
            FighterStatus != Status.Waiting && FighterStatus != Status.Using &&
            FighterStatus != Status.Stunned && !movementsBlocked)
        {
            agent.destination = target.BottomPosition;
            agent.stoppingDistance = target.AbilityRange + 3;
            agent.speed = MovementSpeed;
        }
    }

    void UpdateAbility()
    {
        if (DistanceFrom(target) < AbilityRange && currentAbility != null)
            SelectAbility(currentAbility);
    }

    protected override void UpdateRotation()
    {
        if (!target)
            return;

        Quaternion rotation;
        if (FighterStatus != Status.Free &&
            FighterStatus != Status.Disconnecting)
            rotation = LookAtTargetRotation();
        else
        {
            Quaternion finalRotation = LookAtTargetRotation();
            rotation = Quaternion.Slerp(transform.rotation, finalRotation, 10f * Time.fixedDeltaTime);
        }

        transform.rotation = rotation;
    }

    IEnumerator AbilityAnimationCooldown()
    {
        yield return new WaitForSeconds(2f);
        EndAbility();
    }

    IEnumerator UseAbilityCooldown()
    {
        yield return new WaitForSeconds(2f);
        currentAbility = attacks[Random.Range(0, attacks.Count)];
    }

    public override void EntityUpdate()
    {
        base.EntityUpdate();

        UpdateHealthBar();
    }

    protected virtual void UpdateHealthBar()
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

    public void SetAgentUpdatePosition(bool agentUpdatePosition)
    {
        agent.updatePosition = agentUpdatePosition;
    }

    protected override void UseAbility(Ability ability)
    {
        FighterStatus = Status.Using;
        Effects.FireRay(this, ability);

        StartCoroutine(AbilityAnimationCooldown());
    }

    protected override void OnEndAbility()
    {
        base.OnEndAbility();
        FighterStatus = Status.Free;

        StartCoroutine(UseAbilityCooldown());
    }
}
