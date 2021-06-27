using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Bolt;

public class NetworkManager : GlobalEventListener {

    readonly Dictionary<Guid, Fighter> fighters = new Dictionary<Guid, Fighter>();

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        // Spawn player
        CharacterManager characterManager = GameObject.FindObjectOfType<CharacterManager>();
        PlayerInfo info = new PlayerInfo();
        info.guid = Guid.NewGuid();
        info.name = PlayerPrefs.GetString(PlayerPrefKey.PlayerName);
        info.character = characterManager.CurrentCharacter;
        Transform players = GameObject.Find("Players").transform;
        BoltEntity entity = BoltNetwork.Instantiate(BoltPrefabs.Player, info, players.position, players.rotation);

        // Setup player camera
        GameObject playerCamera = GameObject.Find("PlayerCamera");
        Cinemachine.CinemachineFreeLook cinemachine = playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>();
        cinemachine.Follow = entity.transform;
        cinemachine.LookAt = entity.transform.Find("CameraLookTarget");
    }

    public override void Disconnected(BoltConnection connection)
    {
        BoltLauncher.Shutdown();
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public override void EntityAttached(BoltEntity entity)
    {
        if (entity.StateIs<IFighterState>())
        {
            Fighter fighter = entity.GetComponent<Fighter>();
            fighters[fighter.EntityId] = fighter;
        }
    }

    public override void EntityDetached(BoltEntity entity)
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

    public override void OnEvent(RemoteAbilityEvent evnt)
    {
        RemoteAbility ability = (RemoteAbility) evnt.abilityId;
        Fighter sender = fighters[evnt.senderId];
        Fighter target = fighters[evnt.targetId];
        switch (ability)
        {
            case RemoteAbility.RedAttack2:
                RedAttack2.DoAbility(sender, target);
                break;
            case RemoteAbility.EndAbility:
                target.EndAbility();
                break;
        }
    }
}
