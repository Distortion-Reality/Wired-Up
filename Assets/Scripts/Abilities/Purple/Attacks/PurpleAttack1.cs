using UnityEngine;

public class PurpleAttack1 : Ability
{
    const int power = 10;

    public override int Energy => 10;
    public override AbilityId Id => AbilityId.PurpleAttack1;

    public override void DoAbility(Fighter user, Fighter target)
    {
        int targetHP = target.Stats[StatisticManager.StatisticId.HP].CurrentValue;
        int damage = Effects.CalculateAndApplyDamage(user, target, power);
        Effects.ApplyHealing(user, user, Mathf.Min(targetHP, damage));

        user.EndAbility();
    }
}
