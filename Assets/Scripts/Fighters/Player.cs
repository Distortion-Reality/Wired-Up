using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;
using System;

[RequireComponent(typeof(TargetPlayerAbilityManager))]
[RequireComponent(typeof(CharacterController))]
public class Player : Fighter, IPlayer
{
    CharacterController controller;

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

    static int allyIndex = 0;

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

        PlayerToken info = (PlayerToken) entity.AttachToken;
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

        controller = GetComponent<CharacterController>();

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

        if (entity.HasControl || (BoltNetwork.IsServer && ((PlayerToken) entity.AttachToken).serverController)) // Workaround using token bool, better using a ControlGained method.
        {
            healthBar = GameObject.Find("PlayerHealthBar").GetComponent<Slider>();
            energyBar = GameObject.Find("PlayerEnergyBar").GetComponent<Slider>();
            healthBar.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = color;
        }
        else // Ally
        {
            // Create ally UI
            // TODO: allyIndex is static and is not reset after new game
            float yScale = Screen.height / gui.GetComponent<CanvasScaler>().referenceResolution.y;
            float yOffset = allyIndex++ * allyInfoYOffset * yScale;
            GameObject allyInfo = Instantiate(allyInfoPrefab, parent: gui.transform);
            allyInfo.transform.position += new Vector3(0, yOffset, 0);

            TMPro.TextMeshProUGUI allyName = allyInfo.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            allyName.text = playerName;

            allyInfo.transform.Find("AllyPortrait").GetComponent<Image>().color = color;

            healthBar = allyInfo.GetComponentInChildren<Slider>();
            healthBar.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = color;
        }

        /*if (!entity.IsOwner)
        {
            rb.detectCollisions = false;
            //rb.isKinematic = true;
        }*/
    }

    public override void OwnerUpdate()
    {
        if (!entity.HasControl)
            return;

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

        if (FighterStatus != Status.Stunned)
            UpdateRotation();

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
        if (!entity.HasControl)
            return;
        
        if (gameMenu.IsOpen)
            return;
        
        UpdateMovement();

        //base.OwnerFixedUpdate();
    }

    void CheckDashInput()
    {
        if (Input.GetButtonDown("Dash") && Time.time > nextDashTime &&
            FighterStatus == Status.Free &&
            CheckAndUseEnergy(DashEnergy))
            StartCoroutine(Dash());
    }

    Vector3 dir = Vector3.zero;
    Vector3 oldDir = Vector3.zero;
    bool sendInput = false;
    Quaternion rot = Quaternion.identity;
    Quaternion oldRot = Quaternion.identity;
    bool sendRotation = false;

    public override void ControllerFixedUpdate()
    {
        if (sendInput)
        {
            IPlayerInputCommandInput input = PlayerInputCommand.Create();
            input.Velocity = dir;
            entity.QueueInput(input);
            Debug.Log("Sent input command: " + dir);
            sendInput = false;
            oldDir = dir;
        }
        
        if (sendRotation)
        {
            IPlayerRotateCommandInput rotInput = PlayerRotateCommand.Create();
            rotInput.Rotation = rot;
            entity.QueueInput(rotInput);
            //Debug.Log("Sent rot command: " + rot);
            sendRotation = false;
            oldRot = rot;
        }
    }

    protected override void UpdateMovement()
    {
        // Don't send input if can't move (except the first time to notify it stopped)
        if (FighterStatus == Status.Waiting || FighterStatus == Status.Using ||
            FighterStatus == Status.Stunned || movementsBlocked)
        {
            if (!oldDir.Equals(Vector3.zero))
            {
                dir = Vector3.zero;
                sendInput = true;
            }
        }
        else
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            dir = cam.right * x + cam.forward * z;
            dir.Normalize();
            dir *= movementSpeedMultiplier * MovementSpeed;
            dir.y = 0f;

            // Don't send input if didn't move
            if (dir.Equals(Vector3.zero) && dir.Equals(oldDir))
            {
                sendInput = false;
            }
            else
            {
                sendInput = true;
            }
        }
    }

    /*protected override void UpdateMovement()
    {
        // Don't send input if can't move (except the first time to notify it stopped)
        if (FighterStatus == Status.Waiting || FighterStatus == Status.Using ||
            FighterStatus == Status.Stunned || movementsBlocked)
        {
            if (!oldDir.Equals(Vector3.zero))
            {
                dir = Vector3.zero;
                sendInput = true;
            }
        }
        else
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            dir = cam.right * x + cam.forward * z;
            dir.Normalize();
            dir *= movementSpeedMultiplier * MovementSpeed;

            // Don't send input if didn't move
            if (dir.Equals(Vector3.zero) && dir.Equals(oldDir))
            {
                sendInput = false;
            }
            else
            {
                sendInput = true;
            }
        }
    }*/

    protected override void UpdateRotation()
    {
        rot = CalculateRotation();
        if (!rot.Equals(oldRot))
            sendRotation = true;
        
    }

    public override void ExecuteCommand(Command command, bool resetState)
    {
        //if (!entity.IsOwner)
        //    return;

        if (command is PlayerInputCommand)
        {
            PlayerInputCommand cmd = (PlayerInputCommand) command;
            //Debug.Log("Received input command " + cmd.Input.Velocity + " " + resetState);

            if (resetState)
            {
                Debug.Log("Received reset input command " + cmd.Input.Velocity);
                //rb.velocity = cmd.Result.Velocity;
                //rb.MovePosition(cmd.Result.Position);
                transform.position = cmd.Result.Position;
            }
            else if (command.IsFirstExecution)
            {
                Debug.Log("Received input command " + cmd.Input.Velocity);
                Vector3 vel = cmd.Input.Velocity;
                controller.Move(vel * BoltNetwork.FrameDeltaTime);

                //cmd.Result.Velocity = vel;
                //cmd.Result.Position = rb.position;
                cmd.Result.Position = transform.position;
            }
        }
        else if (command is PlayerRotateCommand)
        {
            PlayerRotateCommand cmd = (PlayerRotateCommand) command;
            //Debug.Log("Received rot command " + cmd.Input.Rotation);
            transform.rotation = cmd.Input.Rotation;
        }
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
