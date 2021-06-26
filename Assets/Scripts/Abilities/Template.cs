using UnityEngine;

public class Template : Ability
{
    const int power = 20;

    public override int Energy => 10;

    public override void DoAbility(Fighter user)
    {
        EndAbility(user);
    }
}
