using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;
using System;

[RequireComponent(typeof(TargetPlayerAbilityManager))]
public class Player : Fighter, IPlayer
{
    string playerName;
    CharacterColor character;

    GameMenu gameMenu;
    GameOver gameOver;
    Slider energyBar;
    public GameObject allyInfoPrefab;
    const float allyInfoYOffset = 100f;

    Transform cam;
    float movementSpeedMultiplier = 1f;
    const int DashEnergy = 20;
    const float DashMultiplier = 2f,
        DashDuration = 0.25f,
        DashCooldown = 4f;
    float nextDashTime = 0f;

    Ability interaction;
    Wire wire;

    public Wire Wire { get => wire; }

    protected new IPlayerState State => entity.GetState<IPlayerState>();
    protected override Quaternion DefaultRotation =>
        new Quaternion(transform.rotation.x, cam.rotation.y, transform.rotation.z, cam.rotation.w);
    public override ParticlesId DamageParticles => ParticlesId.PlayerDamage;
    public override Color CharacterUnityColor { get => character.UnityColor(); }

    public Guid Id => entityId;
    public string Name => playerName;

    protected override void InitStats()
    {
        FighterRangedStatistic hp = new FighterRangedStatistic(100);
        FighterBuffableStatistic armor = new FighterBuffableStatistic(50);
        FighterBuffableStatistic length = new FighterBuffableStatistic(50);
        FighterBuffableStatistic intensity = new FighterBuffableStatistic(50);
        FighterEnergy energy = new FighterEnergy(50);
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

    protected override void UnwrapAttachedToken()
    {
        base.UnwrapAttachedToken();

        PlayerInfo info = (PlayerInfo) entity.AttachToken;
        playerName = info.name;
        character = info.character;
    }

    protected override void LoadModel()
    {
        CharacterManager characterManager = FindObjectOfType<CharacterManager>();
        GameObject prefab = characterManager.GetPrefab(character);
        Instantiate(prefab, parent: transform);
    }

    public override void EntityStart()
    {
        base.EntityStart();

        AbilityRegistry.CharacterAbilities abilities = AbilityRegistry.GetAbilities(character);
        attacks = abilities.attacks;
        assists = abilities.assists;

        // Interaction ability initialization
        interaction = new RedAttack1();

        wire = GetComponentInChildren<Wire>(true);
        CharacterManager characterManager = FindObjectOfType<CharacterManager>();
        foreach (Renderer renderer in wire.GetComponentsInChildren<Renderer>())
            renderer.material = characterManager.GetWireMaterial(character);

        cam = Camera.main.transform;

        // UI initialization
        gameMenu = gui.GetComponent<GameMenu>();
        gameOver = gui.GetComponent<GameOver>();
        Color color = character.UnityColor();

        if (entity.IsOwner)
        {
            healthBar = GameObject.Find("PlayerEnergyBar").GetComponent<Slider>();
            energyBar = GameObject.Find("PlayerHealthBar").GetComponent<Slider>();
            healthBar.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = color;
        }
        else // Ally
        {
            // Create ally UI
            NetworkManager networkManager = GameObject.FindObjectOfType<NetworkManager>();
            float yScale = Screen.height / gui.GetComponent<CanvasScaler>().referenceResolution.y;
            float yOffset = networkManager.AllyCount * allyInfoYOffset * yScale;
            GameObject allyInfo = Instantiate(allyInfoPrefab, parent: gui.transform);
            allyInfo.transform.position += new Vector3(0, yOffset, 0);

            TMPro.TextMeshProUGUI allyName = allyInfo.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            allyName.text = playerName;

            allyInfo.transform.Find("AllyPortrait").GetComponent<Image>().color = color;

            healthBar = allyInfo.GetComponentInChildren<Slider>();
            healthBar.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = color;
        }
    }

    public override void OwnerUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            gameMenu.Trigger();
        if (gameMenu.IsOpen)
            return;

        // Escape button for bugs and glitches
        if (Input.GetKeyDown(KeyCode.P))
        {
            target.TargetAbilityManager.ClearUserAbilityQueue();
            EndAbility();
        }

        base.OwnerUpdate();

        if (FighterStatus != Status.Stunned)
        {
            CheckDashInput();
            
            if (FighterStatus != Status.Using)
            {
                CheckTargetInput();

                if (FighterStatus != Status.Disconnecting && target)
                    CheckAbilityInput();
            }
        }
    }

    public override void OwnerFixedUpdate()
    {
        if (gameMenu.IsOpen)
            return;
        
        UpdateMovement();

        base.OwnerFixedUpdate();
    }

    void CheckDashInput()
    {
        if (Input.GetButtonDown("Dash") && Time.time > nextDashTime &&
            FighterStatus == Status.Free &&
            CheckAndUseEnergy(DashEnergy))
            StartCoroutine(Dash());
    }

    protected override void UpdateMovement()
    {
        Vector3 dir;

        if (FighterStatus == Status.Waiting || FighterStatus == Status.Using ||
            FighterStatus == Status.Stunned || movementsBlocked)
            dir = Vector3.zero;
        else
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            dir = cam.right * x + cam.forward * z;
            dir.Normalize();
            dir *= movementSpeedMultiplier * MovementSpeed;
        }

        dir.y = rb.velocity.y;
        rb.velocity = dir;
    }

    IEnumerator Dash()
    {
        movementSpeedMultiplier = DashMultiplier;
        nextDashTime = Time.time + DashDuration + DashCooldown;
        yield return new WaitForSeconds(DashDuration);

        movementSpeedMultiplier = 1f;
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
                if (hitFighter && DistanceFrom(hitFighter) <= TargetRange && hitFighter != target)
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

    protected override void OnEndAbility()
    {
        base.OnEndAbility();

        if (wire.gameObject.activeSelf)
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
        FighterStatus = Status.Waiting;
        target.TargetAbilityManager.EnqueueUserAbility(this, currentAbility);
        
        wire.StayConnected();
    }

    void CheckAndUpdatePlayerStatus()
    {
        if (FighterStatus == Status.Waiting)
            InterruptWaiting();
        else if (FighterStatus == Status.Connecting)
            EndAbility();
    }

    protected override void EnergyChanged()
    {
        energyBar.value = Energy.PercentageValue;
    }

    protected override void Die()
    {
        base.Die();

        GameLoseEvent.Post(ReliabilityModes.ReliableOrdered, playerName, (int) character);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        return Id.Equals(((IPlayer) obj).Id);
    }
    
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
