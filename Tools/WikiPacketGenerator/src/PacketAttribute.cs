using System;
using System.Collections.Generic;

public class PacketAttribute : Attribute
{
    public Packet.Direction direction;

    public string internalName;
    public ushort packetId;
    public List<ushort> answerPacketIds = null;

    public PacketAttribute(Packet.Direction direction, string internalName, ushort packetId, List<ushort> answerPacketIds)
    {
        this.direction = direction;
        this.internalName = internalName;
        this.packetId = packetId;
        this.answerPacketIds = answerPacketIds;
    }

    public PacketAttribute(Packet.Direction direction, string internalName, ushort packetId, ushort answerPacketIds)
    {
        this.direction = direction;
        this.internalName = internalName;
        this.packetId = packetId;
        this.answerPacketIds = new List<ushort>(){
            answerPacketIds
        };
    }

    public PacketAttribute(Packet.Direction direction, string internalName, ushort packetId)
    {
        this.direction = direction;
        this.internalName = internalName;
        this.packetId = packetId;
        this.answerPacketIds = new List<ushort>();
    }
}