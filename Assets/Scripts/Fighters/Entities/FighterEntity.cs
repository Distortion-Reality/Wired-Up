using Fusion;
using UnityEngine;
using static Fighter;

[RequireComponent(typeof(NetworkObject))]
public abstract class FighterEntity : NetworkBehaviour
{
    protected Fighter fighter;
    GameOver gameOver;

    [Networked(OnChanged = nameof(OnStatusChanged))]
    public Status Status { get; set; }

    [Networked(OnChanged = nameof(OnStatsChanged)), Capacity(5)]
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
        for (int i = 0; i < changed.Behaviour.Stats.Length; i++)
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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UseEnergy(int amount)
    {
        fighter.UseEnergy(amount);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_EnqueueAbility(NetworkId userId, AbilityId abilityId)
    {
        Fighter user = Runner.FindObject(userId).GetComponent<Fighter>();
        Ability ability = AbilityRegistry.Get(abilityId);

        user.Target = fighter;
        fighter.TargetAbilityManager.EnqueueUserAbility(user, ability);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RemoveAbility(NetworkId userId)
    {
        Fighter user = Runner.FindObject(userId).GetComponent<Fighter>();

        fighter.TargetAbilityManager.RemoveUserAbility(user);
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

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SpawnParticle(ParticlesId particleId, Color color)
    {
        Fighter target = fighter;
        Vector3 position;
        switch (particleId)
        {
            case ParticlesId.EnemyDamage:
            case ParticlesId.PlayerDamage:
            case ParticlesId.KirinDamage:
            case ParticlesId.KirinBurst:
            case ParticlesId.Heal:
            case ParticlesId.Death:
                position = target.CentrePosition;
                break;
            case ParticlesId.Buff:
            case ParticlesId.Debuff:
            case ParticlesId.Stun:
                position = target.TopPosition;
                break;
            case ParticlesId.Target:
                position = target.BottomPosition;
                break;
            default:
                position = Vector3.zero;
                break;
        }
        GameObject prefab = NetworkManager.Instance.ParticlesManager.GetParticlesPrefab(particleId);
        GameObject particle = Instantiate(prefab, position, target.transform.rotation);
        ParticleSystem.MainModule main = particle.GetComponent<ParticleSystem>().main;
        main.startColor = new ParticleSystem.MinMaxGradient(color);
        particle.transform.SetParent(target.transform, true);
    }

    public virtual void UnwrapAttachedToken()
    {

    }
}
