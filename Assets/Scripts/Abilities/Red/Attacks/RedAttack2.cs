using System.Collections;
using UnityEngine;
using Photon.Bolt;

public class RedAttack2 : Ability
{
    const int power = 30;

    public override int Energy => 20;
    public override AbilityId Id => AbilityId.RedAttack2;

    public override void DoAbility(Fighter user, Fighter target)
    {
        target.TargetAbilityManager.StartCoroutine(LiftAndSlam(user, target));

        if (user.CompareTag("Player"))
            target.TargetAbilityManager.StartCoroutine(WiresLookAtTarget((Player)user, target));
    }

    IEnumerator LiftAndSlam(Fighter user, Fighter target)
    {
        float targetStartingPositionY = target.transform.position.y;

        bool targetIsEnemy = target.CompareTag("Enemy");
        if (targetIsEnemy)
            ((Enemy)target).SetAgentUpdatePosition(false);

        target.Rb.constraints |= RigidbodyConstraints.FreezePosition;
        target.Rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
        target.Grounded = false;

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            Vector3 translation = 7 * Time.fixedDeltaTime * Vector3.up;
            target.Rb.MovePosition(target.Rb.position + translation);

            elapsedTime += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        while (!target.Grounded)
        {
            Vector3 force = 100 * Vector3.down;
            target.Rb.AddForce(force);

            yield return new WaitForFixedUpdate();
        }

        target.transform.position =
            new Vector3(target.transform.position.x, targetStartingPositionY, target.transform.position.z);
        target.Rb.constraints &= ~RigidbodyConstraints.FreezePosition;
        target.Rb.constraints |= RigidbodyConstraints.FreezePositionY;

        if (targetIsEnemy)
            ((Enemy)target).SetAgentUpdatePosition(true);

        Effects.ApplyDamage(user, target, power);

        user.EndAbility();
    }

    IEnumerator WiresLookAtTarget(Player user, Fighter target)
    {
        Wire wire = user.Wire;
        Quaternion defaultWireParentLocalRotation = wire.transform.parent.localRotation;

        while (!target.Grounded)
        {
            Quaternion rotation = Quaternion.LookRotation(target.BasePosition - wire.transform.position);

            if (user.Entity.IsOwner)
                wire.transform.parent.rotation = rotation;
            else
                ChangeWireRotationEvent.Post(user.Entity.Source, ReliabilityModes.ReliableOrdered,
                    user.EntityId, rotation);
                
                
            yield return null;
        }

        if (user.Entity.IsOwner)
            wire.transform.parent.localRotation = defaultWireParentLocalRotation;
        else
            ChangeWireRotationEvent.Post(user.Entity.Source, ReliabilityModes.ReliableOrdered,
                    user.EntityId, defaultWireParentLocalRotation);
    }
}
