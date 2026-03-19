using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    protected EnemyGroup[] enemyGroups;

    bool charging = false;
    Fighter charged = null;

    bool grounded = true;
    protected bool movementsBlocked = false;

    public FighterEntity Entity { get; set; }
    public Rigidbody Rb { get => rb; set => rb = value; }
    public Dictionary<StatisticManager.StatisticId, FighterStatistic> Stats { get => stats; }
    public Status FighterStatus
    {
        get => Entity.Status;
        set
        {
            if (Entity.Object.HasStateAuthority)
                Entity.Status = value;
            else
                Entity.RPC_SetStatus(value);
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
        Entity = GetComponent<FighterEntity>();

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

        // Setup Networked states
        if (Entity.Object.HasStateAuthority)
        {
            for (int i = 0; i < 5; i++)
            {
                Entity.Stats.Set(i, stats[(StatisticManager.StatisticId)i].CurrentValue);
            }

            Entity.Status = fighterStatus;
        }

        enemyGroups = FindObjectsOfType<EnemyGroup>();
    }

    protected virtual void UnwrapAttachedToken()
    {
        Entity.UnwrapAttachedToken();
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

    public virtual void StateFixedUpdate()
    {
        if (FighterStatus != Status.Stunned)
            UpdateRotation();
    }

    protected virtual void UpdateMovement()
    {

    }

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
        if (Entity.HasStateAuthority)
            OnEndAbility();
        else
            Entity.RPC_EndAbility();
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
        if (Entity.HasStateAuthority)
            ChangeEnergy(-abilityEnergy);
        else
            Entity.RPC_UseEnergy(abilityEnergy);
    }

    public void ChangeStat(StatisticManager.StatisticId statId, int change)
    {
        if (!Entity.Object.HasStateAuthority)
        {
            Entity.RPC_ChangeStat(statId, change);
        }
        else
        {
            Entity.Stats.Set((int) statId, stats[statId].ApplyChange(change));
        }
    }

    public void StatisticChanged(StatisticManager.StatisticId statId, int newValue)
    {
        stats[statId].CurrentValue = newValue;
        if (statId == StatisticManager.StatisticId.HP)
            HpChanged();
    }

    void HpChanged()
    {
        if (healthBar)
            healthBar.value = stats[StatisticManager.StatisticId.HP].PercentageValue;
        
        if (Entity.HasStateAuthority && stats[StatisticManager.StatisticId.HP].CurrentValue == 0)
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

    public void StatusChanged(Status status)
    {
        fighterStatus = status;
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

    protected Quaternion GetRotationUpdate()
    {
        Quaternion rotation;
        if (FighterStatusLocal != Status.Free &&
            FighterStatusLocal != Status.Disconnecting)
            rotation = LookAtTargetRotation();
        else
        {
            Quaternion finalRotation = (target == null) ? DefaultRotation : LookAtTargetRotation();
            rotation = Quaternion.Slerp(rb.rotation, finalRotation, RotationSpeed * Entity.Runner.DeltaTime);
        }

        return rotation;
    }

    protected virtual void UpdateRotation()
    {

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

        Entity.Runner.Despawn(Entity.Object);
    }

    public virtual void EntityDestroyed()
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
