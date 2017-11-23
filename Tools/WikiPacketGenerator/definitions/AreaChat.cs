using System;
using System.Reflection;
using System.ComponentModel;
using System.Linq;
using System.IO;

[Packet(Packet.Direction.ToServer, "Cmd_AreaChat", 571, 572)]
[Description("Area Chat")]
public class AreaChat
{
    [Description("The unique packet identifier")]
    public ushort Id;

    [Unicode()]
    [Length(20)]
    [Description("The type of the chat message")]
    public char[] Type;

    [Unicode()]
    [Length(36)]
    [Description("The username of the sender")]
    public char[] Sender;

    [Description("The length of the message")]
    public ushort MessageLength;

    [Unicode()]
    [Length()]
    [Description("The actual message")]
    public char[] Message;

    public byte[] HexDump()
    {
        /*Id = ((PacketAttribute)typeof(Cmd_AreaChat).GetCustomAttributes(false).ToDictionary(a => a.GetType().Name, a => a)["PacketAttribute"]).packetId;
        Type = "a\x0r\x0e\x0a\x0\x0\x0".ToCharArray();
        Sender = "a\x0d\x0m\x0i\x0n\x0\x0\x0".ToCharArray();
        Message = "h\x0e\x0l\x0l\x0o\x0 \x0w\x0o\x0r\x0l\x0d\x0\x0\x0".ToCharArray();
        MessageLength = (ushort)Message.Length;
        using(var ms = new MemoryStream()){
            using(var writer = new BinaryWriter(ms))
            {
                writer.Write(Id);
                writer.Write(Type);
                writer.Write(Sender);
                writer.Write(MessageLength);
                writer.Write(Message);
            }
            return ms.ToArray();
        }*/

        return new byte[]{
            0x61, 0x00 , 0x72 , 0x00 , 0x65 , 0x00 , 0x61 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00
, 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00
, 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00
, 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x06 , 0x00 , 0x68 , 0x00 , 0x61 , 0x00 , 0x6C , 0x00
, 0x6C , 0x00 , 0x6F , 0x00 , 0x00 , 0x00
        };
        //.GetTypeInfo().GetProperty(<property name>).GetCustomAttribute<YourAttribute>();
    }
}