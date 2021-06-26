using System.Collections;
using UnityEngine;

public class GreenAttack1 : Ability
{
    const float time = 3f;

    public override int Energy => 10;

    public override void DoAbility(Fighter user)
    {
        user.Target.TargetAbilityManager.StartCoroutine(BlockMovements(user.Target));
        EndAbility(user);
    }

    IEnumerator BlockMovements(Fighter target)
    {
        target.MovementsBlocked = true;
        yield return new WaitForSeconds(time);
        target.MovementsBlocked = false;
    }
}
