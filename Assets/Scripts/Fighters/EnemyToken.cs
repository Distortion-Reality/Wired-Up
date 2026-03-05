using UdpKit;

public class EnemyToken : FighterToken
{
    public EnemyId enemyId;
    public override void Write(UdpPacket packet)
    {
        base.Write(packet);

        packet.WriteInt(((int) enemyId));
    }

    public override void Read(UdpPacket packet)
    {
        base.Read(packet);

        enemyId = (EnemyId) packet.ReadInt();
    }
}
