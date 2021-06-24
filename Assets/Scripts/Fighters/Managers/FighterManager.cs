using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class FighterManager<T> : EntityBehaviour<T> where T : IFighterState
{
    protected Fighter fighter;
    protected Rigidbody rb;

    readonly float rotationSpeed = 10f;

    protected Slider healthBar;

    protected float MoveSpeed => 1.3f * Mathf.Log(10 * fighter.Stats[StatisticManager.StatisticId.Spd].CurrentValue);
    protected abstract Quaternion DefaultRotation { get; }
    protected bool IsStunned => fighter.FighterStatus == Fighter.Status.Stunned;

    public override void Attached()
    {
        fighter = GetComponent<Fighter>();
        fighter.FighterStart();

        rb = GetComponent<Rigidbody>();

        state.SetTransforms(state.transform, transform);
        state.hp = fighter.Stats[StatisticManager.StatisticId.HP].CurrentValue;
        state.AddCallback("hp", HpChanged);

        OnAttached();
    }

    void HpChanged()
    {
        fighter.ChangeStat(StatisticManager.StatisticId.HP, state.hp);

        if (healthBar)
            healthBar.value = fighter.Stats[StatisticManager.StatisticId.HP].PercentageValue;

        if (entity.IsOwner && fighter.Stats[StatisticManager.StatisticId.HP].CurrentValue == 0)
            fighter.Die();
    }

    protected abstract void OnAttached();

    // Update is called once per frame
    void Update()
    {
        if (!entity.IsOwner)
            return;

        if (!IsStunned)
            OnUpdate();

        fighter.FighterUpdate();
    }

    protected abstract void OnUpdate();

    public override void SimulateOwner()
    {
        if (IsStunned)
            return;

        OnSimulateOwner();

        UpdateRotation();
    }

    protected abstract void OnSimulateOwner();

    void UpdateRotation()
    {
        Quaternion rotation;
        if (fighter.FighterStatus != Fighter.Status.Free &&
            fighter.FighterStatus != Fighter.Status.Disconnecting)
            rotation = LookAtTargetRotation();
        else
        {
            Quaternion finalRotation = (fighter.Target == null) ? DefaultRotation : LookAtTargetRotation();
            rotation = Quaternion.Slerp(rb.rotation, finalRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        rb.MoveRotation(rotation);
    }

    Quaternion LookAtTargetRotation()
    {
        return Quaternion.LookRotation(
            Vector3.ProjectOnPlane(fighter.Target.transform.position - rb.position, transform.up));
    }
}
