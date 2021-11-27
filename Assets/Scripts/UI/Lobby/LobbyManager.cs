using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Bolt;

public class LobbyManager : GlobalEventListener
{
    Transform canvas;
    
    public float xSpawnPosOffset = 250.0f;
    public bool forceStart = false;
    public bool starting = false;

    public List<LobbyPlayer> AllPlayers { get => NetworkPlayerRegistry.AllPlayers.Select(player => (LobbyPlayer) player.PlayerObject).ToList<LobbyPlayer>(); }

    void Start()
    {
        canvas = GetComponent<Canvas>().transform;
    }

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        // Spawn lobby player
        LobbyPlayerToken lbToken = new LobbyPlayerToken()
        {
            Id = Guid.NewGuid(),
            Name = PlayerPrefs.GetString(PlayerPrefKey.PlayerName)
        };
        BoltEntity entity = BoltNetwork.Instantiate(BoltPrefabs.LobbyPlayer, lbToken);
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
            NetworkPlayerRegistry.CreatePlayer(entity.GetComponent<LobbyPlayer>(), entity.Source);
        }
    }

    public override void OnEvent(GameStartEvent evnt)
    {
        starting = true;
    }

    public override void EntityDetached(BoltEntity entity)
    {
        if (entity.StateIs<ILobbyPlayerState>() && !starting)
        {
            NetworkPlayerRegistry.DestroyPlayer(entity.GetComponent<LobbyPlayer>());
        }
    }

    Vector3 FindAvailableSpawnPosition()
    {
        float x;
        // Owner is always the first
        if (AllPlayers.Count == 1)
        {
            // Choose left
            x = -xSpawnPosOffset;
        }
        else
        {
            // Choose opposite of the occupied slot
            x = -AllPlayers[1].transform.localPosition.x;
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
        NetworkPlayerRegistry.Clear();
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void CheckOwnerCharacterAvailable()
    {
        List<LobbyPlayer> allPlayers = AllPlayers;
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
        List<LobbyPlayer> allPlayers = AllPlayers;
        return allPlayers.Count == 3 && allPlayers.TrueForAll(player => player.IsReady);
    }
}
