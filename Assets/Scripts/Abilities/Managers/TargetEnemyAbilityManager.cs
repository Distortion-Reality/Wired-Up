using System.Collections;
using UnityEngine;

public class TargetEnemyAbilityManager : TargetAbilityManager
{
    public int minAbilities = 2;
    public float waitingTime = 1.5f;

    protected override void CheckUserAbilityQueue(UserAbility userAbility)
    {
        if (usersAreUsing)
        {
            userAbility.UseEnergy();
            userAbility.User.FighterStatus = Fighter.Status.Using; // TODO: send event to change state
        }
        else if (userAbilityQueue.Count == 1)
            StartCoroutine(WaitForOtherAbilities());
    }

    IEnumerator WaitForOtherAbilities()
    {
        float elapsedTime = 0f;

        while (userAbilityQueue.Count > 0 && userAbilityQueue.Count < minAbilities &&
            elapsedTime < waitingTime)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        if (userAbilityQueue.Count < minAbilities)
            EndAbilities();
        else
            StartCoroutine(DoAbilities());
    }

    void EndAbilities()
    {
        foreach (UserAbility userAbility in userAbilityQueue)
            userAbility.User.EndAbility(); // TODO: send EndAbility as event
        userAbilityQueue.Clear();
    }
}
