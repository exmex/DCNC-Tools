using System;
using System.Collections.Generic;

public class PacketField
{
    public string Name;

    public string Type;

    public string Description;

    public int? Size;
}

public class Packet
{
    public enum Direction
    {
        Unknown = 0,
        ToClient,
        ToServer,
    };

    public List<ushort> AnswerPacket = new List<ushort>();

    public ushort Id;
    public Direction Type;

    public string Name;

    public string Description = "";

    public string InternalName;

    public int Size = 2;

    public List<PacketField> Fields = new List<PacketField>();

    public byte[] sample;

    // Dirty fix for variable variable sizes......
    public bool hasUnknownField = false;
}