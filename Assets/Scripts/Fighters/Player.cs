using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

[RequireComponent(typeof(TargetPlayerAbilityManager))]
public class Player : Fighter
{
    Canvas gui;
    public GameObject allyInfoPrefab;
    static int alliesIndex = 0;

    Transform cam;
    float moveSpeedMultiplier = 1f;
    const int DashEnergy = 5;
    const float DashMultiplier = 2f,
        DashDuration = 0.25f,
        DashCooldown = 5f;
    float nextDashTime = 0f;

    Ability interaction;
    Wire wire;

    public Wire Wire { get => wire; }

    protected new IPlayerState State => entity.GetState<IPlayerState>();
    protected override Quaternion DefaultRotation =>
        new Quaternion(transform.rotation.x, cam.rotation.y, transform.rotation.z, cam.rotation.w);

    protected override void InitStats()
    {
        FighterRangedStatistic hp = new FighterRangedStatistic(10);
        FighterBuffableStatistic armor = new FighterBuffableStatistic(10);
        FighterBuffableStatistic length = new FighterBuffableStatistic(50);
        FighterBuffableStatistic intensity = new FighterBuffableStatistic(50);
        FighterEnergy energy = new FighterEnergy(100);
        FighterBuffableStatistic speed = new FighterBuffableStatistic(50);

        stats = new Dictionary<StatisticManager.StatisticId, FighterStatistic>()
        {
            { StatisticManager.StatisticId.HP, hp },
            { StatisticManager.StatisticId.Arm, armor },
            { StatisticManager.StatisticId.Lng, length },
            { StatisticManager.StatisticId.Int, intensity },
            { StatisticManager.StatisticId.Nrg, energy },
            { StatisticManager.StatisticId.Spd, speed }
        };
    }

    public override void EntityStart()
    {
        base.EntityStart();

        // Abilities initialization
        List<Effect> effects1 = new List<Effect>()
        {
            new Damage(10)
        };
        Ability ability1 = new RedAttack1();

        List<Effect> effects2 = new List<Effect>()
        {
            new StatModifier(StatisticManager.StatisticId.Int, 10, 10)
        };
        Ability ability2 = new RedAttack2();

        List<Effect> effects3 = new List<Effect>()
        {
            new Healing(10)
        };
        Ability ability3 = new RedAttack1();

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
        interaction = new RedAttack1();

        wire = GetComponentInChildren<Wire>(true);

        cam = Camera.main.transform;

        // UI initialization
        gui = FindObjectOfType<Canvas>();

        if (entity.IsOwner)
            healthBar = GameObject.Find("PlayerHealthBar").GetComponent<Slider>();
        else // Ally
        {
            // Create ally UI
            Color allyColor = Color.green; // TODO: get actual color
            GameObject allyInfo = Instantiate(allyInfoPrefab, allyInfoPrefab.transform.position, allyInfoPrefab.transform.rotation);
            Vector3 pos = allyInfo.transform.position;
            pos.Set(pos.x, pos.y + alliesIndex * 60, pos.z);
            allyInfo.transform.SetParent(gui.transform, false);

            TMPro.TextMeshProUGUI allyName = allyInfo.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            allyName.text = entity.Source.RemoteEndPoint.SteamId.Id.ToString(); // TODO: get actual name

            allyInfo.transform.Find("AllyPortrait").GetComponent<Image>().color = allyColor;

            healthBar = allyInfo.GetComponentInChildren<Slider>();
            healthBar.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = allyColor;

            alliesIndex++;
        }
    }

    public override void OwnerUpdate()
    {
        base.OwnerUpdate();

        if (fighterStatus != Status.Stunned)
        {
            CheckDashInput();
            
            if (fighterStatus != Status.Using)
            {
                CheckTargetInput();

                if (fighterStatus != Status.Disconnecting && target)
                    CheckAbilityInput();
            }
        }
    }

    void CheckDashInput()
    {
        if (Input.GetButtonDown("Dash") && Time.time > nextDashTime &&
            fighterStatus == Status.Free &&
            CheckAndUseEnergy(DashEnergy))
            StartCoroutine(Dash());
    }

    public override void OwnerFixedUpdate()
    {
        if (fighterStatus != Status.Stunned)
            UpdateMovement();

        base.OwnerFixedUpdate();
    }

    void UpdateMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 dir = cam.right * x + cam.forward * z;
        dir.Normalize();
        dir *= moveSpeedMultiplier * MoveSpeed;
        dir.y = rb.velocity.y;
        rb.velocity = dir;
    }

    IEnumerator Dash()
    {
        moveSpeedMultiplier = DashMultiplier;
        nextDashTime = Time.time + DashDuration + DashCooldown;
        yield return new WaitForSeconds(DashDuration);

        moveSpeedMultiplier = 1f;
    }

    void CheckTargetInput()
    {
        if (Input.GetButtonDown("Target"))
        {
            //Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, LayerMask.GetMask("Player", "Enemy", "Interactable")))
            {
                Fighter hitFighter = hit.collider.GetComponent<Fighter>();
                if (DistanceFrom(hitFighter) <= TargetRange && hitFighter != target)
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

        if (ability != null)
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
