using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using UdpKit;
using System;

public class Menu : GlobalEventListener {

    public void StartServer()
    {
        BoltLauncher.StartServer();
    }

    public override void BoltStartDone()    
    {
        if (BoltNetwork.IsServer)
        {
            string matchName = System.Guid.NewGuid().ToString();
            BoltMatchmaking.CreateSession(sessionID: matchName, sceneToLoad: "Level1Scene");
        }
    }

    public void StartClient()
    {
        BoltLauncher.StartClient();
    }

    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        foreach (var session in sessionList)
        {
            UdpSession photonSession = session.Value;

            if (photonSession.Source == UdpSessionSource.Photon)
            {
                BoltMatchmaking.JoinSession(photonSession);
            }
        }
    }
}
