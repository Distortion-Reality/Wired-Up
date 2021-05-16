using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetEnemyAbilityManager : TargetAbilityManager
{
    public int neededAbilities = 2;
    public int maxAbilities = 3;
    public float waitingTimeSeconds = 3f;

    protected override void CheckUserAbilityQueue()
    {
        int numAbilities = CountUserAbilityQueue();
        if(numAbilities == 1 && numAbilities < neededAbilities)
            StartCoroutine(WaitForOtherAbilities());
    }

    IEnumerator WaitForOtherAbilities()
    {
        yield return new WaitForSeconds(waitingTimeSeconds);

        int numAbilities = CountUserAbilityQueue();
        if(numAbilities < neededAbilities)
            EndAbilities();
        else if(numAbilities < maxAbilities)
            DoAbilities();
        else
            DoRainbowAbility();
    }

    void EndAbilities()
    {
        foreach(UserAbility userAbility in userAbilityQueue)
            userAbility.User.EndAbility();
        userAbilityQueue.Clear();
    }

    void DoRainbowAbility()
    {
        SetUserAbilityStatusUsing();

        // UseRainbowAbility
    }
}
