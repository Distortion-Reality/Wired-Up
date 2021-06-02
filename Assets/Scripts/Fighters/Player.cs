using System.Collections.Generic;
using UnityEngine;

public class Player : Fighter
{
    Wire wire;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // Statistics initialization

        FighterStatistic hp = new FighterStatistic(10);
        FighterStatistic arm = new FighterStatistic(10);
        FighterStatistic spd = new FighterStatistic(10);
        FighterStatistic lng = new FighterStatistic(10);
        FighterStatistic eng = new FighterStatistic(1000000);
        FighterStatistic intensity = new FighterStatistic(10);

        stats = new Dictionary<StatisticManager.StatisticId, FighterStatistic>()
        {
            { StatisticManager.StatisticId.HP, hp },
            { StatisticManager.StatisticId.Arm, arm },
            { StatisticManager.StatisticId.Spd, spd },
            { StatisticManager.StatisticId.Lng, lng },
            { StatisticManager.StatisticId.Eng, eng },
            { StatisticManager.StatisticId.Int, intensity }
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
                        CheckPlayerAbilityStatus();
                        target = hitFighter;
                    }
                }
        }
    }

    void CheckAbilityInput()
    {
        if (target != null)
        {
            if (Input.GetButtonDown("Ability1"))
                UseAbility(attacks[0]);
            else if (Input.GetButtonDown("Ability2"))
                UseAbility(attacks[1]);
            else if (Input.GetButtonDown("Ability3"))
                UseAbility(attacks[2]);
        }
    }

    protected override void UseAbility(Ability ability)
    {
        if (CheckEnergy(ability))
        {
            CheckPlayerAbilityStatus();
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

    void CheckPlayerAbilityStatus()
    {
        if (fighterAbilityStatus == AbilityStatus.WAITING)
            InterruptWaiting();
        else if (fighterAbilityStatus == AbilityStatus.CONNECTING)
            EndAbility();
    }
}
