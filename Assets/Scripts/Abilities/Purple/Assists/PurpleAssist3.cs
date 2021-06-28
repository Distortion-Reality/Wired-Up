using UnityEngine;

public class PurpleAssist3 : Ability
{
    const float percentage = 0.25f;

    public override int Energy => 30;
    public override AbilityId Id => AbilityId.PurpleAssist3;

    public override void DoAbility(Fighter user, Fighter target)
    {
        int hp = Mathf.RoundToInt(percentage * user.Stats[StatisticManager.StatisticId.HP].CurrentValue);
        Effects.ChangeHP(user, user, -hp);
        Effects.ChangeHP(user, target, hp);

        user.EndAbility();
    }
}
