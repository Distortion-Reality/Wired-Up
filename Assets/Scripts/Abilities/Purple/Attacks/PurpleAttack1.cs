using UnityEngine;

public class PurpleAttack1 : Ability
{
    const int power = 10;

    public override int Energy => 10;

    public override void DoAbility(Fighter user)
    {
        int targetHP = user.Target.Stats[StatisticManager.StatisticId.HP].CurrentValue;
        int damage = Effects.Damage(user, user.Target, power);
        Effects.ChangeHP(user.Target, damage);
        Effects.ChangeHP(user, Mathf.Min(targetHP, damage));

        EndAbility(user);
    }
}
