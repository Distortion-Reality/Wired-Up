using Photon.Bolt;

public class NetworkPlayer
{
    BoltConnection connection;
    public IPlayer PlayerObject { get; set; }

    public NetworkPlayer(BoltConnection connection, IPlayer playerObject)
    {
        this.connection = connection;
        PlayerObject = playerObject;
    }

    public bool IsServer
    {
        get => connection == null;
    }

    public bool IsClient
    {
        get => connection != null;
    }
}
