using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Objects;
using Shared.Util;

{{ $description }}
public class {{ $name }}Packet : OutPacket
{
{{ $fields }}
    public override Packet CreatePacket()
    {
        return base.CreatePacket(Packets.CmdMoveVehicle);
    }

    public override int ExpectedSize() => {{ $size }};
    
    public void Send(Client client)
    {
    }
    
    public override byte[] GetBytes()
    {
        using (var ms = new MemoryStream())
        {
            using (var bs = new BinaryWriterExt(ms))
            {

            }
            return ms.ToArray();
        }
    }
}