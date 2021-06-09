using System.Collections.Generic;
using UnityEngine;

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

    // Start is called before the first frame update
    protected virtual void Start()
    {
        defaultRotation = transform.rotation;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        CheckHP();
        UpdateRotation();
    }

    void CheckHP()
    {
        if (stats[StatisticManager.StatisticId.HP].CurrentValue <= 0)
            Die();
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

    void Die()
    {
        Destroy(gameObject);
    }
}
