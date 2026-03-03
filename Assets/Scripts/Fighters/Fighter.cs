using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class Fighter : NetworkBehaviour
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

    protected NetworkObject entity;
    protected Guid entityId;

    float centreOffset, topOffset, bottomOffset;

    protected Rigidbody rb;
    const float RotationSpeed = 10f;

    protected Canvas gui;
    protected Slider healthBar;

    protected Dictionary<StatisticManager.StatisticId, FighterStatistic> stats;
    protected List<Ability> attacks, assists;
    protected Ability currentAbility = null;
    Status fighterStatus = Status.Free;
    TargetAbilityManager targetAbilityManager;
    protected Fighter target = null;
    protected Animator animator;

    bool charging = false;
    Fighter charged = null;

    bool grounded = true;
    protected bool movementsBlocked = false;

    public NetworkObject Entity { get => entity; }
    public Guid EntityId { get => entityId; }
    public Rigidbody Rb { get => rb; set => rb = value; }
    public Dictionary<StatisticManager.StatisticId, FighterStatistic> Stats { get => stats; }
    public Status FighterStatus
    {
        get => (Status) State.status;
        set
        {
            if (entity.HasStateAuthority)
                State.status = (int) value;
            else
                ChangeStatusEvent.Post(entity.Source, ReliabilityModes.ReliableOrdered, entityId, (int) value);
        }
    }
    public Status FighterStatusLocal { get => fighterStatus; set => fighterStatus = value; }
    public TargetAbilityManager TargetAbilityManager { get => targetAbilityManager; }
    public Fighter Target { get => target; set => target = value; }
    public Animator Animator { get => animator; }
    public bool Charging { get => charging; set => charging = value; }
    public Fighter Charged { get => charged; set => charged = value; }
    public bool Grounded { get => grounded; set => grounded = value; }
    public bool MovementsBlocked { set => movementsBlocked = value; }

    protected IFighterState State => entity.GetState<IFighterState>();
    public Vector3 CentrePosition => transform.position + OffsetVector(centreOffset);
    public Vector3 TopPosition => transform.position + OffsetVector(topOffset);
    public Vector3 BottomPosition => transform.position + OffsetVector(bottomOffset);
    protected virtual float MovementSpeed
    {
        get
        {
            float movementSpeed = 1.3f * Mathf.Log(10 * stats[StatisticManager.StatisticId.Spd].CurrentValue);
            float multiplier = (targetAbilityManager.UserAbilityQueueCount > 0) ? 0.5f : 1f;
            return multiplier * movementSpeed;
        }
    }
    protected virtual Quaternion DefaultRotation => transform.rotation;
    protected FighterEnergy Energy => (FighterEnergy) stats[StatisticManager.StatisticId.Nrg];
    public virtual float AbilityRange => 2 * Mathf.Log(10 * stats[StatisticManager.StatisticId.Lng].CurrentValue);
    protected float TargetRange => 2 * AbilityRange;
    public float TargetDistance => DistanceFrom(target);
    public abstract ParticlesId DamageParticles { get; }
    public abstract Color CharacterUnityColor { get; }

    public virtual void EntityStart()
    {
        entity = GetComponent<NetworkObject>();

        UnwrapAttachedToken();

        Collider collider = GetComponent<Collider>();
        float colliderHalfHeight = GetComponent<Collider>().bounds.size.y / 2;
        float colliderCentre = collider.bounds.center.y;
        float colliderTop = colliderCentre + colliderHalfHeight;
        float colliderBottom = colliderCentre - colliderHalfHeight;

        centreOffset = colliderCentre - transform.position.y;
        topOffset = colliderTop - transform.position.y;
        bottomOffset = colliderBottom - transform.position.y;

        rb = GetComponent<Rigidbody>();

        targetAbilityManager = GetComponent<TargetAbilityManager>();

        gui = FindObjectOfType<Canvas>();

        InitStats();
        
        LoadModel();

        animator = GetComponentInChildren<Animator>();

        // Setup Bolt states
        State.SetTransforms(State.transform, transform, transform);
        for (int i = 0; i < 5; i++)
        {
            State.statistics[i] = stats[(StatisticManager.StatisticId) i].CurrentValue;
        }
        if (entity.HasStateAuthority)
            State.status = (int) fighterStatus;

        State.AddCallback("statistics[]", StatisticChanged);
        State.AddCallback("status", StatusChanged);
    }

    protected virtual void UnwrapAttachedToken()
    {
        FighterToken token = (FighterToken) entity.AttachToken;
        entityId = token.guid;
    }

    protected abstract void InitStats();

    protected abstract void LoadModel();

    public virtual void EntityUpdate()
    {
        
    }

    public virtual void OwnerUpdate()
    {                                                                                                                                                                                   
        UpdateTarget();
        RegenEnergy();
    }

    public virtual void OwnerFixedUpdate()
    {
        if (FighterStatus != Status.Stunned)
            UpdateRotation();
    }

    protected abstract void UpdateMovement();

    void UpdateTarget()
    {
        if (target != null && TargetDistance > TargetRange && FighterStatus == Status.Free)
            target = null;
    }

    protected float DistanceFrom(Fighter other)
    {
        return Vector3.Distance(other.BottomPosition, BottomPosition);
    }

    void RegenEnergy()
    {
        if (Energy.CurrentValue < Energy.BaseValue)
        {
            int spd = stats[StatisticManager.StatisticId.Spd].CurrentValue;
            float energyRegen = 2 * Mathf.Log(spd) * Time.deltaTime;
            ChangeEnergy(energyRegen);
        }
    }

    protected void SelectAbility(Ability ability)
    {
        if (CheckAndUseEnergy(ability.Energy))
            UseAbility(ability);
    }

    protected abstract void UseAbility(Ability ability);

    public void EndAbility()
    {
        if (entity.HasStateAuthority)
            OnEndAbility();
        else
            EndAbilityEvent.Post(entity.Source, ReliabilityModes.ReliableOrdered, entityId);
    }

    protected virtual void OnEndAbility()
    {
        currentAbility = null;
        charging = false;
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
        if (entity.HasStateAuthority)
            ChangeEnergy(- abilityEnergy);
        else
            UseEnergyEvent.Post(entity.Source, ReliabilityModes.ReliableOrdered, entityId, abilityEnergy);
    }

    public void ChangeStat(StatisticManager.StatisticId statId, int change)
    {
        if (!entity.HasStateAuthority)
        {
            ChangeStatisticEvent evnt = ChangeStatisticEvent.Create(entity.Source, ReliabilityModes.ReliableOrdered);
            evnt.entityId = entityId;
            evnt.statisticId = (int) statId;
            evnt.change = change;
            evnt.Send();
        }
        else
        {
            State.statistics[(int) statId] = stats[statId].ApplyChange(change);
        }
    }

    void StatisticChanged(IState state, string propertyPath, ArrayIndices arrayIndices)
    {
        int index = arrayIndices[0];
        IFighterState localState = (IFighterState) state;
        int value = localState.statistics[index];

        StatisticManager.StatisticId statId = (StatisticManager.StatisticId) index;
        stats[statId].CurrentValue = value;

        if (statId == StatisticManager.StatisticId.HP)
            HpChanged();
    }

    void HpChanged()
    {
        if (healthBar)
            healthBar.value = stats[StatisticManager.StatisticId.HP].PercentageValue;
        
        if (entity.HasStateAuthority && stats[StatisticManager.StatisticId.HP].CurrentValue == 0)
            Die();
    }

    void ChangeEnergy(float change)
    {
        Energy.ApplyChange(change);
        EnergyChanged();
    }

    protected virtual void EnergyChanged() {}

    public void ApplyStatus(Status status, float time)
    {
        StartCoroutine(ApplyStatusForTime(status, time));
    }

    void StatusChanged()
    {
        fighterStatus = (Status) State.status;
    }

    IEnumerator ApplyStatusForTime(Status status, float time)
    {
        if (FighterStatus != status)
        {
            FighterStatus = status;
            yield return new WaitForSeconds(time);
            FighterStatus = Status.Free;
        }
    }

    protected virtual void UpdateRotation()
    {
        Quaternion rotation;
        if (FighterStatusLocal != Status.Free &&
            FighterStatusLocal != Status.Disconnecting)
            rotation = LookAtTargetRotation();
        else
        {
            Quaternion finalRotation = (target == null) ? DefaultRotation : LookAtTargetRotation();
            rotation = Quaternion.Slerp(rb.rotation, finalRotation, RotationSpeed * Time.fixedDeltaTime);
        }

        rb.MoveRotation(rotation);
    }

    public Quaternion LookAtTargetRotation()
    {
        return Quaternion.LookRotation(
            Vector3.ProjectOnPlane(target.transform.position - rb.position, transform.up));
    }

    Vector3 OffsetVector(float offset)
    {
        return Vector3.up * offset;
    }

    protected virtual void Die()
    {
        Fighter[] fighters = FindObjectsOfType<Fighter>();
        foreach (Fighter fighter in fighters)
            if (fighter.target == this && (fighter.fighterStatus == Status.Connecting ||
                fighter.fighterStatus == Status.Waiting || fighter.fighterStatus == Status.Using))
                fighter.EndAbility();

        Destroy(gameObject);
    }

    public void EntityDestroyed()
    {
        if (healthBar)
            Destroy(healthBar.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        OnChargingCollision(collision);

        if (!grounded && collision.gameObject.CompareTag("Terrain"))
            grounded = true;
    }

    void OnCollisionStay(Collision collision)
    {
        OnChargingCollision(collision);
    }

    void OnChargingCollision(Collision collision)
    {
        if (charging && !collision.gameObject.CompareTag("Terrain"))
        {
            charging = false;
            if (!collision.gameObject.CompareTag(tag) &&
                (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Player")))
                charged = collision.gameObject.GetComponent<Fighter>();
        }
    }
}
