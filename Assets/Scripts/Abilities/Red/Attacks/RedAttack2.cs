using System.Collections;
using UnityEngine;

public class RedAttack2 : Ability
{
    const int power = 30;

    public override int Energy => 30;
    public override AbilityId Id => AbilityId.RedAttack2;

    public override void DoAbility(Fighter user, Fighter target)
    {
        target.TargetAbilityManager.StartCoroutine(LiftAndSlam(user, target));

        if (user is Player player)
        {
            target.TargetAbilityManager.StartCoroutine(WiresLookAtTarget(player, target));
            foreach (Fighter enqueued in target.TargetAbilityManager.UsersInQueue)
                if (enqueued is Player enqueuedPlayer)
                    target.TargetAbilityManager.StartCoroutine(WiresLookAtTarget(enqueuedPlayer, target));
        }
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

        Effects.CalculateAndApplyDamage(user, target, power);

        user.EndAbility();
    }

    IEnumerator WiresLookAtTarget(Player user, Fighter target)
    {
        Wire wire = user.Wire;

        while (!target.Grounded)
        {
            if (user.PlayerEntity.HasStateAuthority)
                wire.transform.parent.LookAt(target.BottomPosition);
            else
                user.PlayerEntity.RPC_ChangeWireRotation(target.BottomPosition, false);
            yield return null;
        }

        if (user.PlayerEntity.HasStateAuthority)
            wire.transform.parent.localRotation = Quaternion.identity;
        else
            user.PlayerEntity.RPC_ChangeWireRotation(Vector3.zero, true);
    }
}
