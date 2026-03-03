using System.Collections;
using UnityEngine;

public class RedAttack1 : Ability
{
    const int power = 15;

    public override int Energy => 10;
    public override AbilityId Id => AbilityId.RedAttack1;

    public override void DoAbility(Fighter user, Fighter target)
    {
        /*
        if (user.Entity.IsOwner)
            DoCharge(user, target);
        else
            ChargeEvent.Post(user.Entity.Source, ReliabilityModes.ReliableOrdered, user.EntityId);
        */

        Effects.CalculateAndApplyDamage(user, target, power);
        DelayEndAbility(user, ParticlesId.PlayerDamage);
    }

    public static void DoCharge(Fighter user, Fighter target)
    {
        target.TargetAbilityManager.StartCoroutine(Charge(user));
    }

    static IEnumerator Charge(Fighter user)
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
