using System.Collections;
using UnityEngine;

public class RedAttack2 : Ability
{
    const int power = 30;

    public override int Energy => 20;

    public override void DoAbility(Fighter user)
    {
        user.Target.TargetAbilityManager.StartCoroutine(LiftAndSlam(user, user.Target));

        if (user.CompareTag("Player"))
            user.Target.TargetAbilityManager.StartCoroutine(WiresLookAtTarget(((Player)user).Wire, user.Target));
    }

    static IEnumerator LiftAndSlam(Fighter user, Fighter target)
    {
        float targetStartingPositionY = target.transform.position.y;
        target.Rb.constraints |= RigidbodyConstraints.FreezePosition;
        target.Rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
        target.Grounded = false;

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            Vector3 translation = 10 * Time.fixedDeltaTime * Vector3.up;
            target.Rb.MovePosition(target.Rb.position + translation);

            elapsedTime += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        while (!target.Grounded)
        {
            Vector3 force = 50 * Vector3.down;
            target.Rb.AddForce(force);

            yield return new WaitForFixedUpdate();
        }

        target.transform.position =
            new Vector3(target.transform.position.x, targetStartingPositionY, target.transform.position.z);
        target.Rb.constraints &= ~RigidbodyConstraints.FreezePosition;
        target.Rb.constraints |= RigidbodyConstraints.FreezePositionY;

        Effects.ApplyDamage(user, target, power);

        user.EndAbility();
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
