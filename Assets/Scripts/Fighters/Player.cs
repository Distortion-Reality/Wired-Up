using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TargetPlayerAbilityManager))]
public class Player : Fighter
{
    Wire wire;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // Statistics initialization

        FighterStatistic hp = new FighterStatistic(10);
        FighterStatistic armor = new FighterStatistic(10);
        FighterStatistic length = new FighterStatistic(10);
        FighterStatistic intensity = new FighterStatistic(10);
        FighterStatistic energy = new FighterStatistic(1000000);
        FighterStatistic speed = new FighterStatistic(10);

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
        Ability ability1 = new Ability(effects1, 5);

        List<Effect> effects2 = new List<Effect>()
        {
            new StatModifier(StatisticManager.StatisticId.Int, 10, 10)
        };
        Ability ability2 = new Ability(effects2, 5);

        List<Effect> effects3 = new List<Effect>()
        {
            new Healing(10)
        };
        Ability ability3 = new Ability(effects3, 5);

        attacks = new List<Ability>()
        {
            ability1,
            ability2,
            ability3
        };

        // Assists initialization
        assists = new List<Ability>();

        // TargetAbilityManager initialization
        targetAbilityManager = GetComponent<TargetPlayerAbilityManager>();

        // Wire initialization
        wire = GetComponentInChildren<Wire>();
        wire.gameObject.SetActive(false);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (fighterAbilityStatus != AbilityStatus.USING)
        {
            CheckTargetInput();
            if (fighterAbilityStatus != AbilityStatus.DISCONNECTING)
                CheckAbilityInput();
        }
    }

    void CheckTargetInput()
    {
        if (Input.GetButtonDown("Target"))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
                if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Player"))
                {
                    Fighter hitFighter = hit.collider.GetComponent<Fighter>();
                    if (hitFighter != target)
                    {
                        CheckAndUpdatePlayerAbilityStatus();
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

        int abilityToUse = 0;
        if (Input.GetButton("AbilityModifier1"))
            abilityToUse = 2;
        else if (Input.GetButton("AbilityModifier2"))
            abilityToUse = 4;

        if (Input.GetButtonDown("Ability2"))
            abilityToUse++;

        if (abilityToUse < abilities.Count)
            UseAbility(abilities[abilityToUse]);
    }

    protected override void UseAbility(Ability ability)
    {
        if (CheckAndUseEnergy(ability.Energy))
        {
            CheckAndUpdatePlayerAbilityStatus();
            wire.Connect(ability);
        }
    }

    public override void EndAbility()
    {
        wire.Disconnect();
    }

    public void InterruptWaiting()
    {
        target.TargetAbilityManager.RemoveUserAbility(this);
        EndAbility();
    }

    public void EnqueueUserAbilityToTarget(Fighter actualTarget, Ability ability)
    {
        target = actualTarget;
        fighterAbilityStatus = AbilityStatus.WAITING;
        target.TargetAbilityManager.EnqueueUserAbility(this, ability);

        wire.StayConnected();
    }

    void CheckAndUpdatePlayerAbilityStatus()
    {
        if (fighterAbilityStatus == AbilityStatus.WAITING)
            InterruptWaiting();
        else if (fighterAbilityStatus == AbilityStatus.CONNECTING)
            EndAbility();
    }
}
