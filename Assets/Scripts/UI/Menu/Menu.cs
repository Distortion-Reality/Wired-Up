using System;
using UnityEngine;
using UdpKit;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;

public class Menu : GlobalEventListener {

    public static string PlayerNameKey = "playerName";
    public TMPro.TMP_InputField playerName;

    void Start()
    {
        PlayerPrefs.DeleteKey(PlayerNameKey);
        if (!PlayerPrefs.HasKey(PlayerNameKey))
        {
            PlayerPrefs.SetString(PlayerNameKey, "Player #" 
            + (((uint) Guid.NewGuid().GetHashCode()).ToString().Substring(0, 4)));
        }
        playerName.text = PlayerPrefs.GetString(PlayerNameKey);
    }

    public void PlayerNameEndEdit(string playerName)
    {
        PlayerPrefs.SetString(PlayerNameKey, playerName);
    }

    public void Host()
    {
        BoltLauncher.StartServer();
    }

    public override void BoltStartDone()
    {
        if (BoltNetwork.IsServer)
        {
            string matchName = Guid.NewGuid().ToString();
            BoltMatchmaking.CreateSession(sessionID: matchName, sceneToLoad: "Lobby");
        }
    }

    public void Join()
    {
        BoltLauncher.StartClient();
    }

    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        foreach (var session in sessionList)
        {
            UdpSession photonSession = session.Value;

            if (photonSession.Source == UdpSessionSource.Photon)
                BoltMatchmaking.JoinSession(photonSession);
        }
    }
}
