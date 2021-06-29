using Photon.Bolt;
using UdpKit;

public class LevelSpawnInfo : IProtocolToken
{
    public CharacterColor center, left, right;
    
    public void Write(UdpPacket packet)
    {
        packet.WriteInt((int) left);
        packet.WriteInt((int) right);
    }

    public void Read(UdpPacket packet)
    {
        left = (CharacterColor) packet.ReadInt();
        right = (CharacterColor) packet.ReadInt();
    }
}
