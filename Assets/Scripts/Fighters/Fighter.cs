using System.Collections;
using System.Collections.Generic;

public abstract class Fighter
{
    public enum AbilityStatus
    {
        FREE,
        EXTENDING,
        WAITING,
        USING
    }

    protected Dictionary<StatisticManager.StatisticId, FighterStatistic> stats;
    protected List<Ability> abilities;
    protected List<Ability> assists;
    protected AbilityStatus fighterAbilityStatus = AbilityStatus.FREE;
    protected TargetAbilityManager targetAbilityManager;
    protected Fighter target = null;

    public Fighter(Dictionary<StatisticManager.StatisticId, FighterStatistic> stats,
        List<Ability> abilities, List<Ability> assists, TargetAbilityManager targetAbilityManager)
    {
        this.stats = stats;
        this.abilities = abilities;
        this.assists = assists;
        this.targetAbilityManager = targetAbilityManager;
    }

    public Dictionary<StatisticManager.StatisticId, FighterStatistic> Stats { get => stats; }

    public List<Ability> Abilities { get => abilities; }

    public List<Ability> Assists { get => assists; }

    public AbilityStatus FighterAbilityStatus { set => fighterAbilityStatus = value; }

    public TargetAbilityManager TargetAbilityManager { get => targetAbilityManager; }

    public Fighter Target
    {
        get => target;
        set
        {
            if(fighterAbilityStatus != AbilityStatus.USING)
            {
                CheckFighterAbilityStatus();
                target = value;
            }
        }
    }

    protected void CheckFighterAbilityStatus()
    {
        if(fighterAbilityStatus == AbilityStatus.WAITING)
            InterruptAbility();
        if(fighterAbilityStatus == AbilityStatus.EXTENDING)
            EndAbility();
    }

    public void TryToUseAbility(Ability ability)
    {
        if(fighterAbilityStatus != AbilityStatus.USING)
            if(stats[StatisticManager.StatisticId.Lng].CurrentValue >= ability.Lng)
            {
                stats[StatisticManager.StatisticId.Lng].CurrentValue -= ability.Lng;
                PrepareToUseAbility(ability);
            }
    }

    protected abstract void PrepareToUseAbility(Ability ability);

    public abstract void UseAbility(Ability ability);

    public virtual void InterruptAbility()
    {
        EndAbility();
    }

    public virtual void EndAbility()
    {
        fighterAbilityStatus = AbilityStatus.FREE;
    }
}
