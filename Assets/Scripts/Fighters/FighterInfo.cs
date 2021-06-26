using System;
using UnityEngine;
using Photon.Bolt;
using UdpKit;

public class FighterInfo : IProtocolToken {
    public Guid guid;
    
    public virtual void Write(UdpPacket packet)
    {
        packet.WriteGuid(guid);
    }

    public virtual void Read(UdpPacket packet)
    {
        guid = packet.ReadGuid();
    }
}
