using System;
using System.IO;

namespace DCNC_Tools.IO
{
    /// <summary>
    /// Class to read XORed binary streams
    /// XOR Method used is OFFSET-BASED XOR
    /// It uses the current position of the stream to get the XOR Key
    /// </summary>
    public class XorBinaryReader : BinaryReader
    {
        /// <summary>
        /// The xor key
        /// </summary>
        private readonly byte[] _xorKey;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="xorKey">The XOR Key to use</param>
        public XorBinaryReader(Stream input, byte[] xorKey) : base(input)
        {
            if(!input.CanSeek) throw new Exception("Need stream that can seek!");
            _xorKey = xorKey;
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
            long xorLength = _xorKey.Length;
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
            var xorKey = _xorKey[xorKeyPos];

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

        /// <summary>
        /// Reads a length prefixed string
        /// </summary>
        /// <returns>XORed string</returns>
        public string ReadStringXor()
        {
            var length = ReadInt32Xor();
            return new string(ReadCharsXor(length));
        }
    }
}