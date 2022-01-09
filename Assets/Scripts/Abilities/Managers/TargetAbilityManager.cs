using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Bolt;

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

        public void DoUserAbility(Fighter target)
        {
            ability.DoAbility(user, target);
        }

        public void UseEnergy()
        {
            user.UseEnergy(ability.Energy);
        }
    }

    protected Fighter target;

    protected Queue<UserAbility> userAbilityQueue = new Queue<UserAbility>();
    protected bool usersAreUsing = false;

    public int UserAbilityQueueCount => userAbilityQueue.Count;
    public IEnumerable<Fighter> UsersInQueue => userAbilityQueue.Select(userAbility => userAbility.User);

    void Start()
    {
        target = GetComponent<Fighter>();
    }

    public Fighter DequeueUserAbilityQueue()
    {
        return userAbilityQueue.Dequeue().User;
    }

    public virtual void EnqueueUserAbility(Fighter user, Ability ability)
    {
        if (target.Entity.IsOwner)
        {
            UserAbility userAbility = new UserAbility(user, ability);
            userAbilityQueue.Enqueue(userAbility);
            CheckUserAbilityQueue(userAbility);
        }
        else
            EnqueueAbilityEvent.Post(target.Entity.Source, ReliabilityModes.ReliableOrdered, user.EntityId,
            target.EntityId, (int) ability.Id);
    }

    public void RemoveUserAbility(Fighter user)
    {
        if (target.Entity.IsOwner)
            userAbilityQueue = new Queue<UserAbility>(userAbilityQueue.Where(
                userAbility => userAbility.User != user));
        else
            RemoveAbilityEvent.Post(target.Entity.Source, ReliabilityModes.ReliableOrdered, user.EntityId, target.EntityId);
    }

    public void ClearUserAbilityQueue()
    {
        userAbilityQueue.Clear();
    }

    protected abstract void CheckUserAbilityQueue(UserAbility userAbility);

    protected IEnumerator DoAbilities()
    {
        SetUsersStatusUsing();

        while (userAbilityQueue.Count > 0)
        {
            UserAbility userAbility = userAbilityQueue.Dequeue();
            userAbility.DoUserAbility(target);

            yield return new WaitUntil(() => userAbility.User.FighterStatusLocal == Fighter.Status.Disconnecting);
        }

        usersAreUsing = false;
    }

    void SetUsersStatusUsing()
    {
        foreach (UserAbility userAbility in userAbilityQueue)
            userAbility.User.FighterStatus = Fighter.Status.Using;

        usersAreUsing = true;
    }
}
