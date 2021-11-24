using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Bolt;

public class LobbyManager : GlobalEventListener
{
    Transform canvas;

    List<LobbyPlayer> allPlayers = new List<LobbyPlayer>(3);

    public float xSpawnPosOffset = 250.0f;
    public bool forceStart = false;

    public List<LobbyPlayer> Players { get => allPlayers; }

    void Start()
    {
        canvas = GetComponent<Canvas>().transform;
    }

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        // Spawn lobby player
        BoltEntity entity = BoltNetwork.Instantiate(BoltPrefabs.LobbyPlayer);
        entity.transform.SetParent(canvas, false);
    }

    public override void EntityAttached(BoltEntity entity)
    {
        if (entity.StateIs<ILobbyPlayerState>())
        {
            if (!entity.IsOwner)
            {
                // Force reset position and rotation because they start with weird values (?)
                entity.transform.localPosition = Vector3.zero;
                entity.transform.localRotation = Quaternion.identity;

                entity.transform.localPosition += FindAvailableSpawnPosition();
                entity.transform.SetParent(canvas, false);
            }
            allPlayers.Add(entity.GetComponent<LobbyPlayer>());
        }
    }

    public override void EntityDetached(BoltEntity entity)
    {
        if (entity.StateIs<ILobbyPlayerState>())
        {
            allPlayers.Remove(entity.GetComponent<LobbyPlayer>());
        }
    }

    Vector3 FindAvailableSpawnPosition()
    {
        float x;
        // Owner is always the first
        if (allPlayers.Count == 1)
        {
            // Choose left
            x = -xSpawnPosOffset;
        }
        else
        {
            // Choose opposite of the occupied slot
            x = -allPlayers[1].transform.localPosition.x;
        }
        return new Vector3(x, 0.0f, 0.0f);
    }

    public override void Disconnected(BoltConnection connection)
    {
        if (BoltNetwork.Server != null && BoltNetwork.Server.Equals(connection))
        {
            ReturnToMenu();
        }
    }

    public void ReturnToMenu()
    {
        BoltLauncher.Shutdown();
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void CheckOwnerCharacterAvailable()
    {
        LobbyPlayer owner = allPlayers[0];
        
        bool available = true;
        for (int i = 1; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].IsReady && allPlayers[i].CurrentCharacter == owner.CurrentCharacter)
            {
                available = false;
            }
        }

        owner.ReadyButton.interactable = available;
    }

    public bool ForceStart(BoltEntity entity)
    {
        return entity.IsOwner && forceStart;
    }

    public bool CanStart()
    {
        return allPlayers.Count == 3 && allPlayers.TrueForAll(player => player.IsReady);
    }
}
