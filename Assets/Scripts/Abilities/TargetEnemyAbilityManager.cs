using System.Collections;
using UnityEngine;

public class TargetEnemyAbilityManager : TargetAbilityManager
{
    public int minAbilities = 2;
    public float waitingTimeSeconds = 2f;

    protected override void CheckUserAbilityQueue(Fighter user)
    {
        if (usersAreUsing)
            user.FighterAbilityStatus = Fighter.AbilityStatus.USING;
        else if (userAbilityQueue.Count == 1 && userAbilityQueue.Count < minAbilities)
            StartCoroutine(WaitForOtherAbilities());
    }

    IEnumerator WaitForOtherAbilities()
    {
        float timeSecondsPassed = 0f;

        while (userAbilityQueue.Count < minAbilities && timeSecondsPassed < waitingTimeSeconds)
        {
            yield return null;
            timeSecondsPassed += Time.deltaTime;
        }

        if (userAbilityQueue.Count < minAbilities)
            EndAbilities();
        else
            StartCoroutine(DoAbilities());
    }

    void EndAbilities()
    {
        foreach (UserAbility userAbility in userAbilityQueue)
            userAbility.User.EndAbility();
        userAbilityQueue.Clear();
    }
}
