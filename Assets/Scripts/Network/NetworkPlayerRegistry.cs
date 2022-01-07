using System.Collections.ObjectModel;
using System.Collections.Generic;
using Photon.Bolt;
using UnityEngine;

public static class NetworkPlayerRegistry
{
    static List<NetworkPlayer> players = new List<NetworkPlayer>(3);

    public static void Clear()
    {
        players.Clear();
    }

    public static NetworkPlayer CreatePlayer(IPlayer playerObject, BoltConnection connection = null)
    {
        NetworkPlayer player = new NetworkPlayer(connection, playerObject);
        
        if (connection != null)
        {
            connection.UserData = player;
            Debug.Log(connection.ConnectionId);
        }

        players.Add(player);
        Debug.Log("Add player " + playerObject.Id);

        return player;
    }

    public static void DestroyPlayer(IPlayer playerObject)
    {
        Debug.Log("Remove player " + playerObject.Id);
        players.Remove(GetPlayer(playerObject));
    }

    public static ReadOnlyCollection<NetworkPlayer> AllPlayers { get => players.AsReadOnly(); }
    public static NetworkPlayer ServerPlayer { get => players.Find(player => player.IsServer); }

    public static NetworkPlayer Self { get => players[0]; } // First player created is always self

    public static NetworkPlayer GetPlayer(BoltConnection connection)
    {
        if (connection == null)
            return ServerPlayer;

        return (NetworkPlayer) connection.UserData;
    }

    public static NetworkPlayer GetPlayer(IPlayer playerObject)
    {
        return players.Find(player => player.PlayerObject.Id.Equals(playerObject.Id));
    }
}
