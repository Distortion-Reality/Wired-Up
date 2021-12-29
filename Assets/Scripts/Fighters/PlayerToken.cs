using UdpKit;

public class PlayerToken : FighterToken
{
    public string name;
    public CharacterColor character;
    public bool serverController;

    public override void Write(UdpPacket packet)
    {
        base.Write(packet);

        packet.WriteString(name);
        packet.WriteInt(((int) character));
        packet.WriteBool(serverController);
    }

    public override void Read(UdpPacket packet)
    {
        base.Read(packet);

        name = packet.ReadString();
        character = (CharacterColor) packet.ReadInt();
        serverController = packet.ReadBool();
    }
}
