using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TargetPlayerAbilityManager))]
public class Player : Fighter
{
    Wire wire;

    protected override void OnFighterStart()
    {
        // Statistics initialization

        FighterRangedStatistic hp = new FighterRangedStatistic(10);
        FighterStatistic armor = new FighterBuffableStatistic(10);
        FighterStatistic length = new FighterBuffableStatistic(50);
        FighterStatistic intensity = new FighterBuffableStatistic(10);
        FighterEnergy energy = new FighterEnergy(100);
        FighterStatistic speed = new FighterBuffableStatistic(50);

        stats = new Dictionary<StatisticManager.StatisticId, FighterStatistic>()
        {
            { StatisticManager.StatisticId.HP, hp },
            { StatisticManager.StatisticId.Arm, armor },
            { StatisticManager.StatisticId.Lng, length },
            { StatisticManager.StatisticId.Int, intensity },
            { StatisticManager.StatisticId.Nrg, energy },
            { StatisticManager.StatisticId.Spd, speed }
        };

        // Abilities initialization

        List<Effect> effects1 = new List<Effect>()
        {
            new Damage(10)
        };
        Ability ability1 = new Ability(effects1, 5, "");

        List<Effect> effects2 = new List<Effect>()
        {
            new StatModifier(StatisticManager.StatisticId.Int, 10, 10)
        };
        Ability ability2 = new Ability(effects2, 5, "");

        List<Effect> effects3 = new List<Effect>()
        {
            new Healing(10)
        };
        Ability ability3 = new Ability(effects3, 5, "");

        attacks = new List<Ability>()
        {
            ability1,
            ability2,
            ability3
        };

        // Assists initialization
        assists = new List<Ability>();

        // Wire initialization
        wire = GetComponentInChildren<Wire>(true);
    }

    protected override void OnUpdate()
    {
        if (fighterStatus != Status.USING)
        {
            CheckTargetInput();
            if (fighterStatus != Status.DISCONNECTING)
                CheckAbilityInput();
        }
    }

    void CheckTargetInput()
    {
        if (Input.GetButtonDown("Target"))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
                if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Player") &&
                    DistanceFrom(hit.transform) <= TargetRange)
                {
                    Fighter hitFighter = hit.collider.GetComponent<Fighter>();
                    if (hitFighter != target)
                    {
                        CheckAndUpdatePlayerStatus();
                        target = hitFighter;
                    }
                }
        }
    }

    void CheckAbilityInput()
    {
        if (target == null)
            return;

        if (!Input.GetButtonDown("Ability1") && !Input.GetButtonDown("Ability2"))
            return;

        List<Ability> abilities = target is Player ? assists : attacks;

        int selectedAbility = 0;
        if (Input.GetButton("AbilityModifier1"))
            selectedAbility = 2;
        else if (Input.GetButton("AbilityModifier2"))
            selectedAbility = 4;

        if (Input.GetButtonDown("Ability2"))
            selectedAbility++;

        if (selectedAbility < abilities.Count)
            SelectAbility(abilities[selectedAbility]);
    }

    protected override void UseAbility(Ability ability)
    {
        CheckAndUpdatePlayerStatus();
        currentAbility = ability;
        wire.Connect();
    }

    public override void EndAbility()
    {
        base.EndAbility();
        wire.Disconnect();
    }

    public void InterruptWaiting()
    {
        target.TargetAbilityManager.RemoveUserAbility(this);
        EndAbility();
    }

    public void EnqueueUserAbilityToTarget(Fighter actualTarget)
    {
        target = actualTarget;
        fighterStatus = Status.WAITING;
        target.TargetAbilityManager.EnqueueUserAbility(this, currentAbility);

        wire.StayConnected();
    }

    void CheckAndUpdatePlayerStatus()
    {
        if (fighterStatus == Status.WAITING)
            InterruptWaiting();
        else if (fighterStatus == Status.CONNECTING)
            EndAbility();
    }
}
