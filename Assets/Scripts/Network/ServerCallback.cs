using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class ServerCallback : Photon.Bolt.GlobalEventListener
{
    public override void Disconnected(BoltConnection connection)
    {
        Debug.Log("Disconnected:" + connection.ConnectionId);
    }

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        Debug.Log("SceneLoadLocalDone: " + scene);
        // Spawn server player
    }

    public override void SceneLoadRemoteDone(BoltConnection connection, IProtocolToken token)
    {
        Debug.Log("SceneLoadRemoteDone: " + connection.ConnectionId);
        // Spawn client player
    }

    /*BoltEntity Spawn(string name, CharacterColor character)
    {
        CharacterManager characterManager = FindObjectOfType<CharacterManager>();
        PlayerInfo info = new PlayerInfo
        {
            guid = Guid.NewGuid(),
            name = PlayerPrefs.GetString(PlayerPrefKey.PlayerName),
            character = characterManager.CurrentCharacter
        };

        LevelSpawnInfo spawnInfo = (LevelSpawnInfo) token;
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
        return entity;
        if (IsServer)
            entity.TakeControl();
        else
            entity.AssignControl(connection);
    }*/
}
