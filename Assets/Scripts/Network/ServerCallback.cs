using UnityEngine;
using Photon.Bolt;

[BoltGlobalBehaviour(BoltNetworkModes.Server, "Level2Scene")]
public class ServerCallback : Photon.Bolt.GlobalEventListener
{
    float playerSpawnOffset = 5;

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        // Spawn server player
        BoltEntity entity = SpawnPlayer(null);
        entity.TakeControl();
    }

    public override void SceneLoadRemoteDone(BoltConnection connection, IProtocolToken token)
    {
        // Spawn client player
        BoltEntity entity = SpawnPlayer(connection);
        entity.AssignControl(connection);
    }

    BoltEntity SpawnPlayer(BoltConnection connection = null)
    {
        LobbyPlayer player = (LobbyPlayer) NetworkPlayerRegistry.GetPlayer(connection).PlayerObject;
        PlayerToken token = new PlayerToken
        {
            guid = player.Id,
            name = player.Name,
            character = (CharacterColor) player.CurrentCharacter,
            serverController = connection == null 
        };

        Transform spawnPoint = GameObject.Find("PlayersSpawnPoint").transform;
        Vector3 spawnPosition = spawnPoint.position;
        if (!BoltNetwork.IsServer)
        {
            spawnPosition += -spawnPoint.right * playerSpawnOffset;
            playerSpawnOffset *= -1; // spawn next player right
        }

        return BoltNetwork.Instantiate(BoltPrefabs.Player, token, spawnPosition, spawnPoint.rotation);
    }
}
