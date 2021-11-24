using UdpKit;

public class PlayerInfo : FighterInfo
{
    public string name;
    public CharacterColor character;

    public override void Write(UdpPacket packet)
    {
        base.Write(packet);

        packet.WriteString(name);
        packet.WriteInt(((int) character));
    }

    public override void Read(UdpPacket packet)
    {
        base.Read(packet);

        name = packet.ReadString();
        character = (CharacterColor) packet.ReadInt();
    }
}
