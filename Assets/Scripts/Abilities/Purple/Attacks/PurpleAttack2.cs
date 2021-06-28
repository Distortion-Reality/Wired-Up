using System.Collections;
using UnityEngine;

public class PurpleAttack2 : Ability
{
    const int power = 5;
    const float rate = 1f;
    const int times = 5;

    public override int Energy => 20;
    public override AbilityId Id => AbilityId.PurpleAttack2;

    public override void DoAbility(Fighter user, Fighter target)
    {
        target.TargetAbilityManager.StartCoroutine(DamageOverTime(user, target));
        user.EndAbility();
    }

    static IEnumerator DamageOverTime(Fighter user, Fighter target)
    {
        for (int i = 0; i < times; i++)
        {
            Effects.ApplyDamage(user, target, power);
            yield return new WaitForSeconds(rate);
        }
    }
}
