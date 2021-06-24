using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TargetPlayerAbilityManager))]
public class Player : Fighter
{
    Ability interaction;
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

        // Interaction ability initialization
        List<Effect> interactionEffects = new List<Effect>();
        interaction = new Ability(interactionEffects, 5, "");

        // Wire initialization
        wire = GetComponentInChildren<Wire>(true);
    }

    protected override void OnFighterUpdate()
    {
        if (fighterStatus != Status.Using)
        {
            CheckTargetInput();
            if (fighterStatus != Status.Disconnecting && target)
                CheckAbilityInput();
        }
    }

    void CheckTargetInput()
    {
        if (Input.GetButtonDown("Target"))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            if (Physics.Raycast(ray, out RaycastHit hit, LayerMask.GetMask("Player", "Enemy", "Interactable")))
                if (DistanceFrom(hit.transform) <= TargetRange)
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
        int abilityIndex;
        if (Input.GetButtonDown("Ability1"))
            abilityIndex = 0;
        else if (Input.GetButtonDown("Ability2"))
            abilityIndex = 1;
        else
            return;

        int skillSet = 0;
        if (Input.GetButton("SkillSet2"))
            skillSet = 1;
        else if (Input.GetButton("SkillSet3"))
            skillSet = 2;

        Ability ability = null;
        if (target.CompareTag("Interactable") && abilityIndex == 1 && skillSet == 0)
            ability = interaction;
        else
        {
            List<Ability> abilities;
            int offset;
            if (target.CompareTag("Player"))
            {
                abilities = assists;
                offset = 1;
            }
            else
            {
                abilities = attacks;
                offset = 2;
            }

            abilityIndex = offset * skillSet + abilityIndex;
            if (abilityIndex < abilities.Count)
                ability = abilities[abilityIndex];
        }

        if (ability)
            SelectAbility(ability);
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
        fighterStatus = Status.Waiting;
        target.TargetAbilityManager.EnqueueUserAbility(this, currentAbility);

        wire.StayConnected();
    }

    void CheckAndUpdatePlayerStatus()
    {
        if (fighterStatus == Status.Waiting)
            InterruptWaiting();
        else if (fighterStatus == Status.Connecting)
            EndAbility();
    }
}
