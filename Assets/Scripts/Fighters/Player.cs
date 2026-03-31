using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TargetPlayerAbilityManager))]
[RequireComponent(typeof(PlayerEntity))]
public class Player : Fighter
{
    public PlayerEntity PlayerEntity => Entity as PlayerEntity;
    string PlayerName => PlayerEntity.PlayerName.Value;
    CharacterColor Character => PlayerEntity.Character;

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

    protected override Quaternion DefaultRotation =>
        new Quaternion(transform.rotation.x, cam.rotation.y, transform.rotation.z, cam.rotation.w);
    public override ParticlesId DamageParticles => ParticlesId.PlayerDamage;
    public override Color CharacterUnityColor { get => Character.UnityColor(); }

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

    protected override void LoadModel()
    {
        CharacterManager characterManager = FindObjectOfType<CharacterManager>();
        GameObject prefab = characterManager.GetPrefab(Character);
        Instantiate(prefab, parent: transform);
    }

    public override void EntityStart()
    {
        base.EntityStart();

        AbilityRegistry.CharacterAbilities abilities = AbilityRegistry.GetAbilities(Character);
        attacks = abilities.attacks;
        assists = abilities.assists;

        // Interaction ability initialization
        interaction = new RedAttack1();

        wire = GetComponentInChildren<Wire>(true);
        CharacterManager characterManager = FindObjectOfType<CharacterManager>();
        foreach (Renderer renderer in wire.GetComponentsInChildren<Renderer>())
            renderer.material = characterManager.GetWireMaterial(Character);

        wire.Init();

        // Setup player camera
        if (Entity.HasInputAuthority)
        {
            GameObject playerCamera = GameObject.Find("PlayerCamera");
            Cinemachine.CinemachineFreeLook cinemachine = playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>();
            cinemachine.Follow = PlayerEntity.transform;
            cinemachine.LookAt = PlayerEntity.transform.Find("CameraLookTarget");
        }

        cam = Camera.main.transform;

        // UI initialization
        gameMenu = gui.GetComponent<GameMenu>();
        gameOver = gui.GetComponent<GameOver>();
        Color color = Character.UnityColor();

        if (Entity.HasInputAuthority)
        {
            healthBar = GameObject.Find("PlayerEnergyBar").GetComponent<Slider>();
            energyBar = GameObject.Find("PlayerHealthBar").GetComponent<Slider>();
            healthBar.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = color;
        }
        else // Ally
        {
            // Create ally UI
            NetworkManager networkManager = FindObjectOfType<NetworkManager>();
            float yScale = Screen.height / gui.GetComponent<CanvasScaler>().referenceResolution.y;
            float yOffset = networkManager.AllyCount * allyInfoYOffset * yScale;
            GameObject allyInfo = Instantiate(allyInfoPrefab, parent: gui.transform);
            allyInfo.transform.position += new Vector3(0, yOffset, 0);

            TMPro.TextMeshProUGUI allyName = allyInfo.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            allyName.text = PlayerName;

            allyInfo.transform.Find("AllyPortrait").GetComponent<Image>().color = color;

            healthBar = allyInfo.GetComponentInChildren<Slider>();
            healthBar.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = color;
        }

        foreach (EnemyGroup enemyGroup in enemyGroups)
        {
            enemyGroup.RegisterPlayer(this);
        }

        BossArenaEntrance.Instance.RegisterPlayer(this);
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

        if (FighterStatusLocal != Status.Stunned)
        {
            CheckDashInput();
            
            if (FighterStatusLocal != Status.Using)
            {
                CheckTargetInput();

                if (FighterStatusLocal != Status.Disconnecting && target)
                    CheckAbilityInput();
            }
        }
    }

    void CheckDashInput()
    {
        if (Input.GetButtonDown("Dash") && Time.time > nextDashTime &&
            FighterStatusLocal == Status.Free &&
            CheckAndUseEnergy(DashEnergy))
            StartCoroutine(Dash());
    }

    public Vector3 GetMovementInput()
    {
        Vector3 dir;

        if (FighterStatusLocal == Status.Waiting || FighterStatusLocal == Status.Using ||
            FighterStatusLocal == Status.Stunned || movementsBlocked)
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

        return dir;
    }

    public void Move(Vector3 dir)
    {
        rb.velocity = dir;
        Speed = rb.velocity.magnitude;
    }

    IEnumerator Dash()
    {
        movementSpeedMultiplier = DashMultiplier;
        nextDashTime = Time.time + DashDuration + DashCooldown;
        yield return new WaitForSeconds(DashDuration);

        movementSpeedMultiplier = 1f;
    }

    public Quaternion GetRotationInput()
    {
        return GetRotationUpdate();
    }

    public void Rotate(Quaternion rotation)
    {
        rb.MoveRotation(rotation);
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
        PlayerEntity.RPC_WireConnect();
    }

    protected override void OnEndAbility()
    {
        base.OnEndAbility();

        if (wire.gameObject.activeSelf)
        {
            FighterStatusLocal = Status.Disconnecting;
            PlayerEntity.RPC_WireDisconnect();
        }
    }

    public void InterruptWaiting()
    {
        target.TargetAbilityManager.RemoveUserAbility(this);
        EndAbility();
    }

    public void EnqueueUserAbilityToTarget(Fighter actualTarget)
    {
        target = actualTarget;
        target.TargetAbilityManager.EnqueueUserAbility(this, currentAbility);
        
        PlayerEntity.RPC_WireStayConnected(target.Entity.Id);
    }

    void CheckAndUpdatePlayerStatus()
    {
        if (FighterStatusLocal == Status.Waiting)
            InterruptWaiting();
        else if (FighterStatusLocal == Status.Connecting)
            EndAbility();
    }

    protected override void EnergyChanged()
    {
        energyBar.value = Energy.PercentageValue;
    }

    protected override void Die()
    {
        NetworkManager.RPC_GameLose(PlayerEntity.Runner, PlayerName, Character);

        base.Die();
    }

    public override void EntityDestroyed()
    {
        base.EntityDestroyed();

        foreach (EnemyGroup enemyGroup in enemyGroups)
        {
            enemyGroup.UnregisterPlayer(this);
        }

        BossArenaEntrance.Instance.UnregisterPlayer(this);
    }
}
