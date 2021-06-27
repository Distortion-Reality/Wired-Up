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

        public void DoUserAbility()
        {
            ability.DoAbility(user);
        }

        public void UseEnergy()
        {
            user.UseEnergy(ability.Energy); // TODO: send event to user
        }
    }

    protected Queue<UserAbility> userAbilityQueue = new Queue<UserAbility>();
    protected bool usersAreUsing = false;

    public void EnqueueUserAbility(Fighter user, Ability ability)
    {
        UserAbility userAbility = new UserAbility(user, ability);
        userAbilityQueue.Enqueue(userAbility);
        CheckUserAbilityQueue(userAbility);
    }

    public void RemoveUserAbility(Fighter user)
    {
        userAbilityQueue = new Queue<UserAbility>(userAbilityQueue.Where(
            userAbility => userAbility.User != user));
    }

    protected abstract void CheckUserAbilityQueue(UserAbility userAbility);

    protected IEnumerator DoAbilities()
    {
        SetUsersAbilityStatusUsing();

        while (userAbilityQueue.Count > 0)
        {
            UserAbility userAbility = userAbilityQueue.Dequeue();
            userAbility.DoUserAbility();

            yield return new WaitWhile(() => userAbility.User.FighterStatus == Fighter.Status.Using);
            // TODO: get fighter status state
        }

        usersAreUsing = false;
    }

    void SetUsersAbilityStatusUsing()
    {
        foreach (UserAbility userAbility in userAbilityQueue)
            userAbility.User.FighterStatus = Fighter.Status.Using; // TODO: send event

        usersAreUsing = true;
    }
}
