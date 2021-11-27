using System;
using Photon.Bolt;
using UdpKit;

public class LobbyPlayerToken : IProtocolToken
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public void Read(UdpPacket packet)
    {
        Id = packet.ReadGuid();
        Name = packet.ReadString();
    }

    public void Write(UdpPacket packet)
    {
        packet.WriteGuid(Id);
        packet.WriteString(Name);
    }
}
