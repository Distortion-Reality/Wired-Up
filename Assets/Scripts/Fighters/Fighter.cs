using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;
using System;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class Fighter : MonoBehaviour
{
    public enum AbilityStatus
    {
        FREE,
        CONNECTING,
        WAITING,
        USING,
        DISCONNECTING
    }

    Quaternion defaultRotation;
    readonly float rotationSpeed = 10f;

    protected Dictionary<StatisticManager.StatisticId, FighterStatistic> stats;
    protected List<Ability> attacks, assists;
    protected AbilityStatus fighterAbilityStatus = AbilityStatus.FREE;
    protected TargetAbilityManager targetAbilityManager;
    protected Fighter target = null;

    public Dictionary<StatisticManager.StatisticId, FighterStatistic> Stats { get => stats; }
    public AbilityStatus FighterAbilityStatus { get => fighterAbilityStatus; set => fighterAbilityStatus = value; }
    public TargetAbilityManager TargetAbilityManager { get => targetAbilityManager; }
    public Fighter Target { get => target; }

    protected IFighterState state;

    public virtual void Init()
    {
        defaultRotation = transform.rotation;

        state = GetComponent<BoltEntity>().GetState<IFighterState>();
    }

    public virtual void UpdateFrame()
    {
        UpdateRotation();
    }

    void UpdateRotation()
    {
        if (fighterAbilityStatus != AbilityStatus.FREE &&
            fighterAbilityStatus != AbilityStatus.DISCONNECTING)
            transform.rotation = LookAtTargetRotation();
        else
        {
            Quaternion finalRotation;
            if (target == null)
                finalRotation = defaultRotation;
            else
                finalRotation = LookAtTargetRotation();

            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, rotationSpeed * Time.deltaTime);
        }
    }

    Quaternion LookAtTargetRotation()
    {
        return Quaternion.LookRotation(
            Vector3.ProjectOnPlane(target.transform.position - transform.position, Vector3.up));
    }

    protected abstract void UseAbility(Ability ability);

    public abstract void EndAbility();

    public bool CheckAndUseEnergy(int abilityEnergy)
    {
        if (stats[StatisticManager.StatisticId.Nrg].CurrentValue >= abilityEnergy)
        {
            UseEnergy(abilityEnergy);
            return true;
        }

        return false;
    }

    public void UseEnergy(int abilityEnergy)
    {
        stats[StatisticManager.StatisticId.Nrg].CurrentValue -= abilityEnergy;
    }

    public void Damage(int amount)
    {
        int newHp = Math.Max(0, stats[StatisticManager.StatisticId.HP].CurrentValue - amount);
        state.hp = newHp;
        if (newHp == 0)
            Die();
    }

    public void Heal(int amount)
    {
        int maxHp = stats[StatisticManager.StatisticId.HP].BaseValue;
        int newHp = Math.Min(maxHp, stats[StatisticManager.StatisticId.HP].CurrentValue + amount);
        state.hp = newHp;
    }

    void Die()
    {
        BoltNetwork.Destroy(gameObject);
    }
}
