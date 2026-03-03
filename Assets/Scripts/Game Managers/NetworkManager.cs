using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkRunner runner;

    ParticlesManager particlesManager;
    GameOver gameOver;

    readonly Dictionary<Guid, Fighter> fighters = new Dictionary<Guid, Fighter>();
    int allyCount = -1;

    public int AllyCount { get => allyCount; }

    void Start()
    {
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.AddCallbacks(this);

        particlesManager = GetComponent<ParticlesManager>();
        gameOver = FindObjectOfType<GameOver>();
    }

    public override void Disconnected(BoltConnection connection)
    {
        BoltLauncher.Shutdown();
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public override void EntityAttached(NetworkObject entity)
    {
        if (entity.StateIs<IFighterState>())
        {
            Fighter fighter = entity.GetComponent<Fighter>();
            fighters[fighter.EntityId] = fighter;

            if (fighter is Player)
                allyCount++;
        }
    }

    public override void EntityDetached(NetworkObject entity)
    {
        if (entity.StateIs<IFighterState>())
        {
            Fighter fighter = entity.GetComponent<Fighter>();
            fighters.Remove(fighter.EntityId);
        }
    }

    public override void OnEvent(ChangeStatisticEvent evnt)
    {
        Fighter fighter = fighters[evnt.entityId];
        fighter.ChangeStat((StatisticManager.StatisticId) evnt.statisticId, evnt.change);
    }

    public override void OnEvent(ChangeStatusEvent evnt)
    {
        Fighter fighter = fighters[evnt.entityId];
        fighter.FighterStatus = (Fighter.Status) evnt.statusId;
    }

    public override void OnEvent(UseEnergyEvent evnt)
    {
        Fighter fighter = fighters[evnt.entityId];
        fighter.UseEnergy(evnt.amount);
    }

    public override void OnEvent(EnqueueAbilityEvent evnt)
    {
        Ability ability = AbilityRegistry.Get((AbilityId) evnt.abilityId);
        Fighter target = fighters[evnt.targetId];
        Fighter user = fighters[evnt.senderId];
        user.Target = target;
        target.TargetAbilityManager.EnqueueUserAbility(user, ability);
    }

    public override void OnEvent(RemoveAbilityEvent evnt)
    {
        fighters[evnt.targetId].TargetAbilityManager.RemoveUserAbility(fighters[evnt.senderId]);
    }

    public override void OnEvent(EndAbilityEvent evnt)
    {
        fighters[evnt.entityId].EndAbility();
    }

    public override void OnEvent(ChargeEvent evnt)
    {
        Fighter user = fighters[evnt.entityId];
        RedAttack1.DoCharge(user, user.Target);
    }

    public override void OnEvent(ChangeWireRotationEvent evnt)
    {
        Transform wire = ((Player) fighters[evnt.entityId]).Wire.transform.parent;
        if (evnt.reset)
            wire.transform.localRotation = Quaternion.identity;
        else
            wire.transform.LookAt(evnt.lookAt);
    }

    public override void OnEvent(WireConnectEvent evnt)
    {
        ((Player) fighters[evnt.entityId]).Wire.Connect();
    }

    public override void OnEvent(WireStayConnectedTargetEvent evnt)
    {
        Player sender = (Player) fighters[evnt.senderId];
        sender.Target = fighters[evnt.targetId];
        sender.Wire.StayConnected();
    }

    public override void OnEvent(WireDisconnectEvent evnt)
    {
        ((Player) fighters[evnt.entityId]).Wire.Disconnect();
    }

    public override void OnEvent(SpawnParticleEvent evnt)
    {
        ParticlesId id = (ParticlesId) evnt.particleId;
        Fighter target = fighters[evnt.entityId];
        Vector3 position;
        switch (id)
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
        GameObject prefab = particlesManager.GetParticlesPrefab(id);
        GameObject particle = Instantiate(prefab, position, target.transform.rotation);
        ParticleSystem.MainModule main = particle.GetComponent<ParticleSystem>().main;
        main.startColor = new ParticleSystem.MinMaxGradient(evnt.color);
        particle.transform.SetParent(target.transform, true);
    }

    public override void OnEvent(GameLoseEvent evnt)
    {
        gameOver.Lose(evnt.playerName, (CharacterColor) evnt.character);
    }

    public override void OnEvent(GameWinEvent evnt)
    {
        gameOver.Win();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Destroy(GameObject.Find("Menu Audio"));

        // Spawn player
        CharacterManager characterManager = FindObjectOfType<CharacterManager>();
        PlayerToken info = new PlayerToken
        {
            guid = Guid.NewGuid(),
            name = PlayerPrefs.GetString(PlayerPrefKey.PlayerName),
            character = characterManager.CurrentCharacter
        };

        LevelSpawnToken spawnInfo = (LevelSpawnToken)token;
        Transform spawnPoint = GameObject.Find("PlayersSpawnPoint").transform;
        Vector3 spawnPosition = spawnPoint.position;
        if (!BoltNetwork.IsServer)
        {
            if (characterManager.CurrentCharacter == spawnInfo.left)
                spawnPosition += Vector3.left * 5;
            else if (characterManager.CurrentCharacter == spawnInfo.right)
                spawnPosition += Vector3.right * 5;
        }

        BoltEntity entity = BoltNetwork.Instantiate(BoltPrefabs.Player, info, spawnPosition, spawnPoint.rotation);

        // Setup player camera
        GameObject playerCamera = GameObject.Find("PlayerCamera");
        Cinemachine.CinemachineFreeLook cinemachine = playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>();
        cinemachine.Follow = entity.transform;
        cinemachine.LookAt = entity.transform.Find("CameraLookTarget");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }
}
