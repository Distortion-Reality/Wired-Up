using UnityEngine;

public class Template : Ability
{
    public override int Energy => 0;

    public override void DoAbility(Fighter user)
    {
        EndAbility(user);
    }
}
