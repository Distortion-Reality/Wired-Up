using System.Collections;
using UnityEngine;
using Photon.Bolt;

public class RedAttack2 : Ability
{
    const int power = 30;

    public override int Energy => 20;

    public override void DoAbility(Fighter user)
    {
        if (user.Target.Entity.IsOwner)
            DoAbility(user, user.Target);
        else
        {
            RemoteAbilityEvent evnt = RemoteAbilityEvent.Create(user.Target.Entity.Source, ReliabilityModes.ReliableOrdered);
            evnt.senderId = user.EntityId;
            evnt.targetId = user.Target.EntityId;
            evnt.abilityId = (int) RemoteAbility.RedAttack2;
            evnt.Send();
        }

        if (user.CompareTag("Player"))
            user.Target.TargetAbilityManager.StartCoroutine(WiresLookAtTarget(((Player)user).Wire, user.Target));
    }

    public static void DoAbility(Fighter sender, Fighter target)
    {
        target.TargetAbilityManager.StartCoroutine(LiftAndSlam(sender, target));
    }

    static IEnumerator LiftAndSlam(Fighter user, Fighter target)
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

        if (user.Entity.IsOwner)
            user.EndAbility();
        else
        {
            RemoteAbilityEvent evnt = RemoteAbilityEvent.Create(user.Entity.Source, ReliabilityModes.ReliableOrdered);
            evnt.senderId = target.EntityId;
            evnt.targetId = user.EntityId;
            evnt.abilityId = (int) RemoteAbility.EndAbility;
            evnt.Send();
        }
    }

    IEnumerator WiresLookAtTarget(Wire wire, Fighter target)
    {
        Quaternion defaultWireParentLocalRotation = wire.transform.parent.localRotation;

        while (!target.Grounded)
        {
            wire.transform.parent.LookAt(target.BasePosition);
            yield return null;
        }

        wire.transform.parent.localRotation = defaultWireParentLocalRotation;
    }
}
