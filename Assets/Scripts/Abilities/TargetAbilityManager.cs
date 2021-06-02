using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class TargetAbilityManager : MonoBehaviour
{
    protected class UserAbility
    {
        readonly Fighter user;
        readonly Ability ability;

        public UserAbility(Fighter user, Ability ability)
        {
            this.user = user;
            this.ability = ability;
        }

        public Fighter User => user;
        public Ability Ability => ability;
    }

    protected Queue<UserAbility> userAbilityQueue = new Queue<UserAbility>();
    protected bool usersAreUsing = false;

    public void EnqueueUserAbility(Fighter user, Ability ability)
    {
        userAbilityQueue.Enqueue(new UserAbility(user, ability));
        CheckUserAbilityQueue(user);
    }

    public void RemoveUserAbility(Fighter user)
    {
        userAbilityQueue = new Queue<UserAbility>(userAbilityQueue.Where(
            userAbility => userAbility.User != user));
    }

    protected abstract void CheckUserAbilityQueue(Fighter user);

    protected IEnumerator DoAbilities()
    {
        SetUsersAbilityStatusUsing();

        while (userAbilityQueue.Count > 0)
        {
            UserAbility userAbility = userAbilityQueue.Dequeue();
            userAbility.Ability.DoAbility(userAbility.User);

            yield return new WaitWhile(() => userAbility.User.FighterAbilityStatus == Fighter.AbilityStatus.USING);
        }

        usersAreUsing = false;
    }

    void SetUsersAbilityStatusUsing()
    {
        foreach (UserAbility userAbility in userAbilityQueue)
            userAbility.User.FighterAbilityStatus = Fighter.AbilityStatus.USING;

        usersAreUsing = true;
    }
}
