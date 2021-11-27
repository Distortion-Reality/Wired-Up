using System.Collections.ObjectModel;
using System.Collections.Generic;
using Photon.Bolt;

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
            connection.UserData = player;   

        players.Add(player);

        return player;
    }

    public static void DestroyPlayer(IPlayer playerObject)
    {
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
        return players.Find(player => player.PlayerObject.Equals(playerObject));
    }
}
