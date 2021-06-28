using System.Collections;
using UnityEngine;

public class GreenAssist3 : Ability
{
    const float percentage = 0.05f;
    const float rate = 1f;
    const int times = 5;
    const float radius = 4f;

    public override int Energy => 40;

    public override void DoAbility(Fighter user)
    {
        int selfDamage = user.Stats[StatisticManager.StatisticId.HP].CurrentValue / 4;
        Effects.ChangeHP(user, selfDamage);

        user.Target.TargetAbilityManager.StartCoroutine(HealingOverTimeAOE(user.Target));

        EndAbility(user);
    }

    IEnumerator HealingOverTimeAOE(Fighter target)
    {
        for (int i = 0; i < times; i++)
        {
            Collider[] colliders = Effects.AreaOfEffect(target, radius);
            foreach (Collider collider in colliders)
            {
                Fighter fighter = collider.GetComponent<Fighter>();
                Effects.ApplyHealing(fighter, percentage);
            }

            yield return new WaitForSeconds(rate);
        }
    }
}
