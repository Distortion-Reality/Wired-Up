using UnityEngine;

public class PurpleAssist3 : Ability
{
    const float percentage = 0.25f;

    public override int Energy => 30;

    public override void DoAbility(Fighter user)
    {
        int hp = Mathf.RoundToInt(percentage * user.Stats[StatisticManager.StatisticId.HP].CurrentValue);
        Effects.ChangeHP(user, -hp);
        Effects.ChangeHP(user.Target, hp);

        user.EndAbility();
    }
}
