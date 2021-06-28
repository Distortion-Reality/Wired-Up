using System.Collections;
using UnityEngine;

public class GreenAttack1 : Ability
{
    const float time = 3f;

    public override int Energy => 10;
    public override AbilityId Id => AbilityId.GreenAttack1;

    public override void DoAbility(Fighter user, Fighter target)
    {
        target.TargetAbilityManager.StartCoroutine(BlockMovements(target));
        user.EndAbility();
    }

    static IEnumerator BlockMovements(Fighter target)
    {
        target.MovementsBlocked = true;
        yield return new WaitForSeconds(time);
        target.MovementsBlocked = false;
    }
}
