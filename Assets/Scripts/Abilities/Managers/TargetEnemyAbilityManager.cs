using System.Collections;
using UnityEngine;

public class TargetEnemyAbilityManager : TargetAbilityManager
{
    public int minAbilities = 2;
    public float waitingTime = 1.5f;

    public override void EnqueueUserAbility(Fighter user, Ability ability)
    {
        base.EnqueueUserAbility(user, ability);

        if (userAbilityQueue.Count < minAbilities && user is Player player)
            Effects.SpawnParticles(target, ParticlesId.Target, player.CharacterUnityColor);
    }

    protected override void CheckUserAbilityQueue(UserAbility userAbility)
    {
        if (usersAreUsing)
        {
            userAbility.UseEnergy();
            userAbility.User.FighterStatus = Fighter.Status.Using;
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
            userAbility.User.EndAbility();
        userAbilityQueue.Clear();
    }
}
