using Fusion;
using UnityEngine;
using static Fighter;

[RequireComponent(typeof(NetworkObject))]
public abstract class FighterEntity : NetworkBehaviour
{
    const int STATS_COUNT = 5;

    public Fighter fighter;
    GameOver gameOver;

    [Networked(OnChanged = nameof(OnStatusChanged))]
    public Status Status { get; set; }

    [Networked(OnChanged = nameof(OnStatsChanged)), Capacity(STATS_COUNT)]
    public NetworkArray<int> Stats => default;

    public FighterToken Token { get; set; }

    public override void Spawned()
    {
        gameOver = FindObjectOfType<GameOver>();

        fighter = GetComponent<Fighter>();
        fighter.EntityStart();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        fighter.EntityDestroyed();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameOver != null && gameOver.IsGameOver)
        {
            fighter.EntityDestroyed();
            return;
        }
        
        if (Object.HasInputAuthority)
            fighter.OwnerUpdate();
        
        fighter.EntityUpdate();
    }

    // FixedUpdateNetwork is a FixedUpdate (this is run only if Object.HasStateAuthority)
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || (gameOver != null && gameOver.IsGameOver))
            return;
        
        fighter.StateFixedUpdate();
    }

    static void OnStatusChanged(Changed<FighterEntity> changed)
    {
        changed.Behaviour.fighter.StatusChanged(changed.Behaviour.Status);
    }

    static void OnStatsChanged(Changed<FighterEntity> changed)
    {
        for (int i = 0; i < STATS_COUNT; i++)
        {
            changed.LoadOld();
            int oldValue = changed.Behaviour.Stats[i];
            changed.LoadNew();
            int newValue = changed.Behaviour.Stats[i];

            if (oldValue != newValue)
            {
                changed.Behaviour.fighter.StatisticChanged((StatisticManager.StatisticId)i, newValue);
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetStatus(Status status)
    {
        fighter.FighterStatus = status;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ChangeStat(StatisticManager.StatisticId statId, int change)
    {
        fighter.ChangeStat(statId, change);
    }

    [Rpc(RpcSources.All, RpcTargets.InputAuthority)]
    public void RPC_UseEnergy(int amount)
    {
        fighter.UseEnergy(amount);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_EnqueueAbility(NetworkBehaviourId userId, AbilityId abilityId)
    {
        if (Runner.TryFindBehaviour(userId, out FighterEntity userEntity))
        {
            Fighter user = userEntity.fighter;
            Ability ability = AbilityRegistry.Get(abilityId);

            user.Target = fighter;
            fighter.TargetAbilityManager.EnqueueUserAbility(user, ability);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RemoveAbility(NetworkBehaviourId userId)
    {
        if (Runner.TryFindBehaviour(userId, out FighterEntity userEntity))
        {
            Fighter user = userEntity.fighter;

            fighter.TargetAbilityManager.RemoveUserAbility(user);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_EndAbility()
    {
        fighter.EndAbility();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Charge()
    {
        RedAttack1.DoCharge(fighter, fighter.Target);
    }

    public virtual void UnwrapAttachedToken()
    {

    }
}
