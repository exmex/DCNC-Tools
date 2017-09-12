using System;
using System.IO;
using System.Text;

namespace NayaPack
{
    /// <summary>
    /// Class to read XORed binary streams
    /// XOR Method used is OFFSET-BASED XOR
    /// It uses the current position of the stream to get the XOR Key
    /// </summary>
    public class XorBinaryReader : BinaryReader
    {
        /// <summary>
        /// The XOR Key
        /// </summary>
        public static readonly byte[] Xor = {
            0x01, 0x05, 0x06, 0x02, 0x04, 0x03, 0x07, 0x08, 0x01, 0x05, 0x06, 0x0f, 0x04, 0x03, 0x07, 0x0c, 0x31, 0x85,
            0x76, 0x39, 0x34, 0x3d, 0x30, 0xe8, 0x67, 0x36, 0x36, 0x32, 0x3e, 0x33, 0x34, 0x3b, 0x11, 0x15, 0x16, 0x16,
            0x14, 0x13, 0x1d, 0x18, 0x11, 0x03, 0x06, 0x0c, 0x04, 0x03, 0x06, 0x08, 0x2e, 0x55, 0x26, 0x23, 0x2a, 0x23,
            0x2e, 0x28, 0x21, 0x21, 0x26, 0x27, 0x2e, 0x00, 0x2d, 0x2d, 0xcf, 0xa5, 0x06, 0x02, 0x04, 0x0f, 0x07, 0x18,
            0xe1, 0x15, 0x36, 0x18, 0x60, 0x13, 0x1a, 0x19, 0x11, 0x15, 0x16, 0x10, 0x12, 0x13, 0x17, 0x38, 0xf1, 0x25
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="input">The stream to read from</param>
        public XorBinaryReader(Stream input) : base(input)
        {
            if(!input.CanSeek) throw new Exception("Need stream that can seek!");
        }
        
        /// <summary>
        /// Reads a 2-Byte (16-Bit) Int16/Short
        /// </summary>
        /// <returns>XORed int16</returns>
        public short ReadInt16Xor()
        {
            var intBytes = ReadBytesXor(2);
            return BitConverter.ToInt16(intBytes, 0);
        }

        /// <summary>
        /// Reads an 4-Byte (32-Bit) XORed Int32/integer
        /// </summary>
        /// <returns>XORed integer</returns>
        public int ReadInt32Xor()
        {
            var intBytes = ReadBytesXor(4);
            return BitConverter.ToInt32(intBytes, 0);
        }
        
        /// <summary>
        /// Reads an 8-Byte (64-Bit) XORed Int64/long
        /// </summary>
        /// <returns>XORed integer</returns>
        public long ReadInt64Xor()
        {
            var intBytes = ReadBytesXor(8);
            return BitConverter.ToInt64(intBytes, 0);
        }

        /// <summary>
        /// Reads a single XORed byte
        /// </summary>
        /// <returns>XORed byte</returns>
        public byte ReadByteXor()
        {
            long xorKeyPos = 0;
            long xorLength = Xor.Length;
            var pos = BaseStream.Position;
            if (pos >= (xorLength) && pos % (xorLength) != 0)
            {
                xorKeyPos = pos % xorLength;
            }
            else if (pos >= xorLength)
            {
                xorKeyPos = 0;
            }
            else
                xorKeyPos = pos;
            var xorKey = Xor[xorKeyPos];

            var b = ReadByte();
            var xorB = (byte) (b ^ xorKey);

            return xorB;
        }

        /// <summary>
        /// Reads X amounts of XORed bytes
        /// </summary>
        /// <param name="count">The amount of bytes to read</param>
        /// <returns>XORed byte array</returns>
        public byte[] ReadBytesXor(int count)
        {
            var b = new byte[count];
            for (var i = 0; i < count; i++)
                b[i] = ReadByteXor();
            return b;
        }

        /// <summary>
        /// Reads count amounts of XORed chars
        /// </summary>
        /// <param name="count">The amount of chars to read</param>
        /// <returns>XOred char array</returns>
        public char[] ReadCharsXor(int count)
        {
            var b = new char[count];
            for (var i = 0; i < count; i++)
                b[i] = ReadCharXor();
            return b;
        }

        /// <summary>
        /// Reads a single XORed char
        /// </summary>
        /// <returns>XORed char</returns>
        public char ReadCharXor() => (char)ReadByteXor();

        public string ReadStringXor()
        {
            var length = ReadInt32Xor();
            return new string(ReadCharsXor(length));
        }
    }
}