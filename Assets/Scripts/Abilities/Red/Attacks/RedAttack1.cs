using System.Collections;
using UnityEngine;

public class RedAttack1 : Ability
{
    const int power = 20;

    public override int Energy => 10;
    public override AbilityId Id => AbilityId.RedAttack1;

    public override void DoAbility(Fighter user, Fighter target)
    {
        target.TargetAbilityManager.StartCoroutine(Charge(user));
    }

    IEnumerator Charge(Fighter user)
    {
        user.Charging = true;
        while (user.Charging)
        {
            Vector3 force = 20 * user.Stats[StatisticManager.StatisticId.Spd].CurrentValue * Vector3.forward;
            user.Rb.AddRelativeForce(force);
            
            yield return new WaitForFixedUpdate();
        }

        if (user.Charged)
        {
            Effects.CalculateAndApplyDamage(user, user.Charged, power);
            user.Charged = null;
        }

        user.EndAbility();
    }
}
