using UnityEngine;

public class YellowAssist2 : Ability
{
    public override int Energy => 20;

    public override void DoAbility(Fighter user)
    {
        EndAbility(user);
    }
}
