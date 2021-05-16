using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class TargetAbilityManager : MonoBehaviour
{
    protected class UserAbility
    {
        Fighter user;
        Ability ability;

        public UserAbility(Fighter user, Ability ability)
        {
            this.user = user;
            this.ability = ability;
        }

        public Fighter User { get => user; }
        public Ability Ability { get => ability; }
    }

    protected Queue<UserAbility> userAbilityQueue = new Queue<UserAbility>();

    public void EnqueueUserAbility(Fighter user, Ability ability)
    {
        userAbilityQueue.Enqueue(new UserAbility(user, ability));
        CheckUserAbilityQueue();
    }

    protected abstract void CheckUserAbilityQueue();

    public int CountUserAbilityQueue()
    {
        return userAbilityQueue.Count;
    }

    public void DequeueUserAbility(Fighter user)
    {
        userAbilityQueue = new Queue<UserAbility>(userAbilityQueue.Where(
            userAbility => userAbility.User == user));
    }

    protected void DoAbilities()
    {
        SetUserAbilityStatusUsing();

        foreach(UserAbility userAbility in userAbilityQueue)
            userAbility.Ability.DoAbility(userAbility.User);

        userAbilityQueue.Clear();
    }

    protected void SetUserAbilityStatusUsing()
    {
        foreach(UserAbility userAbility in userAbilityQueue)
            userAbility.User.FighterAbilityStatus = Fighter.AbilityStatus.USING;
    }
}
