using System.Collections;
using System.Collections.Generic;

public class TargetPlayerAbilityManager : TargetAbilityManager
{
    protected override void CheckUserAbilityQueue()
    {
        DoAbilities();
    }
}
