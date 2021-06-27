using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TargetEnemyAbilityManager))]
public class Enemy : Fighter
{
    NavMeshAgent agent;

    protected new IEnemyState State => entity.GetState<IEnemyState>();
    protected override float MovementSpeed => 1.5f * base.MovementSpeed;
    public override float AbilityRange => 1.5f * base.AbilityRange;

    protected override void InitStats()
    {
        FighterRangedStatistic hp = new FighterRangedStatistic(20);
        FighterBuffableStatistic armor = new FighterBuffableStatistic(50);
        FighterBuffableStatistic length = new FighterBuffableStatistic(50);
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
        Ability attack1 = new RedAttack1();
        Ability attack2 = new RedAttack2();

        attacks = new List<Ability>()
        {
            attack1,
            attack2,
        };

        // Assists initialization
        Ability assist1 = new BlueAssist2();

        assists = new List<Ability>()
        {
            assist1
        };

        agent = GetComponent<NavMeshAgent>();
    }

    public override void OwnerUpdate()
    {
        base.OwnerUpdate();

        UpdateMovement();
    }

    protected override void UpdateMovement()
    {
        if (target &&
            fighterStatus != Status.Waiting && fighterStatus != Status.Using &&
            fighterStatus != Status.Stunned && !movementsBlocked)
        {
            agent.destination = target.BasePosition;
            agent.stoppingDistance = target.AbilityRange + 3;
            agent.speed = MovementSpeed;
        }
    }

    public void SetAgentUpdatePosition(bool agentUpdatePosition)
    {
        agent.updatePosition = agentUpdatePosition;
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
