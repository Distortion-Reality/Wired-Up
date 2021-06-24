using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
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
    protected IFighterState state;

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

    public Dictionary<StatisticManager.StatisticId, FighterStatistic> Stats { get => stats; }
    public Status FighterStatus { get => fighterStatus; set => fighterStatus = value; }
    public TargetAbilityManager TargetAbilityManager { get => targetAbilityManager; }
    public Fighter Target { get => target; }
    public Animator Animator { get => animator; }

    FighterEnergy Energy => (FighterEnergy) stats[StatisticManager.StatisticId.Nrg];
    public float AbilityRange => 2 * Mathf.Log(10 * stats[StatisticManager.StatisticId.Lng].CurrentValue);
    protected float TargetRange => 2 * AbilityRange;
    public float TargetDistance => DistanceFrom(target.transform);

    public void FighterStart()
    {
        state = GetComponent<BoltEntity>().GetState<IFighterState>();

        targetAbilityManager = GetComponent<TargetAbilityManager>();
        animator = GetComponent<Animator>();

        OnFighterStart();
    }

    protected abstract void OnFighterStart();

    // Update is called once per frame
    public void FighterUpdate()
    {
        UpdateTarget();
        RegenEnergy();

        if (fighterStatus != Status.Stunned)
            OnFighterUpdate();
    }

    protected abstract void OnFighterUpdate();

    void UpdateTarget()
    {
        if (target != null && TargetDistance > TargetRange && fighterStatus == Status.Free)
            target = null;
    }

    protected float DistanceFrom(Transform other)
    {
        return Vector3.Magnitude(Vector3.ProjectOnPlane(other.position - transform.position, transform.up));
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

    public void ApplyAbilityEffects()
    {
        currentAbility.ApplyEffects(this);
    }

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

    public void ChangeHP(int change)
    {
        state.hp = stats[StatisticManager.StatisticId.HP].CurrentValue;
    }

    void ChangeEnergy(float change)
    {
        Energy.ApplyChange(change);
    }

    public void ApplyStatus(Status status, float time)
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

    public void Die()
    {
        BoltNetwork.Destroy(gameObject);
    }
}
