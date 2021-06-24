using UnityEngine;
using Photon.Bolt;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class FighterManager<T> : EntityBehaviour<T> where T : IFighterState
{
    protected Fighter fighter;

    protected Rigidbody rb;

    readonly float rotationSpeed = 10f;

    protected float MoveSpeed => 1.3f * Mathf.Log(10 * fighter.Stats[StatisticManager.StatisticId.Spd].CurrentValue);
    protected abstract Quaternion DefaultRotation { get; }
    protected bool IsStunned => fighter.FighterStatus == Fighter.Status.STUNNED;

    public override void Attached()
    {
        fighter = GetComponent<Fighter>();
        fighter.FighterStart();

        rb = GetComponent<Rigidbody>();

        state.SetTransforms(state.transform, transform);

        OnAttached();
    }

    protected abstract void OnAttached();

    // Update is called once per frame
    void Update()
    {
        if (IsStunned)
            return;

        OnUpdate();
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
        if (fighter.FighterStatus != Fighter.Status.FREE &&
            fighter.FighterStatus != Fighter.Status.DISCONNECTING)
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
