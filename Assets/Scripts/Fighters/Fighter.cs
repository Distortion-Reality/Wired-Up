using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class Fighter : MonoBehaviour
{
    public enum Status
    {
        Free,
        Connecting,
        Waiting,
        Using,
        Disconnecting,
        Stunned
    }

    protected BoltEntity entity;
    protected Guid entityId;

    Vector3 pivotOffset;
    protected Rigidbody rb;
    const float RotationSpeed = 10f;

    protected Slider healthBar;

    protected Dictionary<StatisticManager.StatisticId, FighterStatistic> stats;
    protected List<Ability> attacks, assists;
    protected Ability currentAbility = null;
    protected Status fighterStatus = Status.Free;
    TargetAbilityManager targetAbilityManager;
    protected Fighter target = null;
    Animator animator;

    bool charging = false;
    Fighter charged = null;

    bool grounded = true;
    protected bool movementsBlocked = false;

    public Rigidbody Rb { get => rb; set => rb = value; }
    public Dictionary<StatisticManager.StatisticId, FighterStatistic> Stats { get => stats; }
    public Status FighterStatus { get => fighterStatus; set => fighterStatus = value; }
    public TargetAbilityManager TargetAbilityManager { get => targetAbilityManager; }
    public Fighter Target { get => target; }
    public Animator Animator { get => animator; }
    public bool Charging { get => charging; set => charging = value; }
    public Fighter Charged { get => charged; set => charged = value; }
    public bool Grounded { get => grounded; set => grounded = value; }
    public bool MovementsBlocked { set => movementsBlocked = value; }

    protected IFighterState State => entity.GetState<IFighterState>();
    public Vector3 BasePosition => transform.position - pivotOffset;
    protected float MoveSpeed => 1.3f * Mathf.Log(10 * stats[StatisticManager.StatisticId.Spd].CurrentValue);
    protected abstract Quaternion DefaultRotation { get; }
    FighterEnergy Energy => (FighterEnergy) stats[StatisticManager.StatisticId.Nrg];
    public float AbilityRange => 2 * Mathf.Log(10 * stats[StatisticManager.StatisticId.Lng].CurrentValue);
    protected float TargetRange => 2 * AbilityRange;
    public float TargetDistance => DistanceFrom(target);

    public virtual void EntityStart()
    {
        entity = GetComponent<BoltEntity>();
        entityId = ((FighterInfo) entity.AttachToken).guid;

        pivotOffset = new Vector3(0, transform.position.y, 0);
        rb = GetComponent<Rigidbody>();

        targetAbilityManager = GetComponent<TargetAbilityManager>();
        animator = GetComponentInChildren<Animator>();

        InitStats();

        // Setup Bolt states
        State.SetTransforms(State.transform, transform);
        State.hp = stats[StatisticManager.StatisticId.HP].CurrentValue;
        State.AddCallback("hp", HpChanged);
    }

    protected abstract void InitStats();

    // Update is called once per frame
    public virtual void OwnerUpdate()
    {                                                                                                                                                                                   
        UpdateTarget();
        RegenEnergy();
    }

    public virtual void OwnerFixedUpdate()
    {
        if (fighterStatus != Status.Stunned)
            UpdateRotation();
    }

    void UpdateTarget()
    {
        if (target != null && TargetDistance > TargetRange && fighterStatus == Status.Free)
            target = null;
    }

    protected float DistanceFrom(Fighter other)
    {
        return Vector3.Distance(other.BasePosition, BasePosition);
    }

    void RegenEnergy()
    {
        if (Energy.CurrentValue < Energy.BaseValue)
        {
            int spd = stats[StatisticManager.StatisticId.Spd].CurrentValue;
            float energyRegen = 2.20738f * Mathf.Log(0.809275f * spd) * Time.deltaTime;
            ChangeEnergy(energyRegen);
        }
    }

    protected void SelectAbility(Ability ability)
    {
        if (CheckAndUseEnergy(ability.Energy))
            UseAbility(ability);
    }

    protected abstract void UseAbility(Ability ability);

    public virtual void EndAbility()
    {
        currentAbility = null;
    }

    public bool CheckAndUseEnergy(int abilityEnergy)
    {
        bool enoughEnergy = Energy.CurrentValue >= abilityEnergy;
        if (enoughEnergy)
            UseEnergy(abilityEnergy);

        return enoughEnergy;
    }

    public void UseEnergy(int abilityEnergy)
    {
        ChangeEnergy(- abilityEnergy);
    }

    public void ChangeStat(StatisticManager.StatisticId statId, int change)
    {
        stats[statId].ApplyChange(change);
    }

    public void ChangeHp(int change)
    {
        State.hp = stats[StatisticManager.StatisticId.HP].ApplyChange(change);
    }

    void HpChanged()
    {
        if (!entity.IsOwner)
            stats[StatisticManager.StatisticId.HP].CurrentValue = State.hp;

        if (healthBar)
            healthBar.value = stats[StatisticManager.StatisticId.HP].PercentageValue;
        
        if (entity.IsOwner && stats[StatisticManager.StatisticId.HP].CurrentValue == 0)
            Die();
    }

    void ChangeEnergy(float change)
    {
        Energy.ApplyChange(change);
    }

    public void ApplyStatus(Status status, float time = 5f)
    {
        StartCoroutine(ApplyStatusForTime(status, time));
    }

    IEnumerator ApplyStatusForTime(Status status, float time)
    {
        if (fighterStatus != status)
        {
            fighterStatus = status;

            yield return new WaitForSeconds(time);

            fighterStatus = Status.Free;
        }
    }

    void UpdateRotation()
    {
        Quaternion rotation;
        if (fighterStatus != Status.Free &&
            FighterStatus != Status.Disconnecting)
            rotation = LookAtTargetRotation();
        else
        {
            Quaternion finalRotation = (target == null) ? DefaultRotation : LookAtTargetRotation();
            rotation = Quaternion.Slerp(rb.rotation, finalRotation, RotationSpeed * Time.fixedDeltaTime);
        }

        rb.MoveRotation(rotation);
    }

    Quaternion LookAtTargetRotation()
    {
        return Quaternion.LookRotation(
            Vector3.ProjectOnPlane(target.transform.position - rb.position, transform.up));
    }

    void Die()
    {
        BoltNetwork.Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (charging && !collision.gameObject.CompareTag("Terrain"))
        {
            charging = false;
            if (collision.gameObject.CompareTag("Enemy"))
                charged = collision.gameObject.GetComponent<Enemy>();
        }

        if (!grounded && collision.gameObject.CompareTag("Terrain"))
            grounded = true;
    }
}
