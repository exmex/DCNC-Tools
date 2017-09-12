using System;
using System.Collections.Generic;
using System.IO;

namespace DCNC_Tools.IO
{
    /// <summary>
    /// Class to read XORed binary streams
    /// XOR Method used is OFFSET-BASED XOR
    /// It uses the current position of the stream to get the XOR Key
    /// </summary>
    public class XorBinaryWriter : BinaryWriter
    {
        /// <summary>
        /// The xor key
        /// </summary>
        private readonly byte[] _xorKey;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="input">The stream to read from</param>
        /// <param name="xorKey">The XOR key to use</param>
        public XorBinaryWriter(Stream input, byte[] xorKey) : base(input)
        {
            if(!input.CanSeek) throw new Exception("Need stream that can seek!");
            _xorKey = xorKey;
        }

        /// <summary>
        /// Writes a 2-Byte (16-Bit) Int16/Short
        /// </summary>
        public void WriteXor(short i)
        {
            var bs = BitConverter.GetBytes(i);
            WriteXor(bs);
        }

        /// <summary>
        /// Writes an 4-Byte (32-Bit) XORed Int32/integer
        /// </summary>
        public void WriteXor(int i)
        {
            var bs = BitConverter.GetBytes(i);
            WriteXor(bs);
        }
        
        /// <summary>
        /// Writes an 8-Byte (64-Bit) XORed Int64/long
        /// </summary>
        public void WriteXor(long i)
        {
            var bs = BitConverter.GetBytes(i);
            WriteXor(bs);
        }

        public void WriteXor(byte b)
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
            
            var xorB = (byte) (b ^ xorKey);
            
            Write(xorB);
        }

        /// <summary>
        /// Writes a Byte array
        /// </summary>
        /// <param name="bs"></param>
        public void WriteXor(IEnumerable<byte> bs)
        {
            foreach (var b in bs)
            {
                WriteXor(b);
            }
        }

        /// <summary>
        /// Writes a char
        /// </summary>
        public void WriteXor(char c)
        {
            WriteXor((byte)c);
        }

        /// <summary>
        /// Writes a Char array
        /// </summary>
        public void WriteXor(char[] cs)
        {
            foreach (var c in cs)
                WriteXor(c);
        }

        /// <summary>
        /// Writes a length prefixed string
        /// </summary>
        public void WriteXor(string s, bool lengthPrefix = true)
        {
            if(lengthPrefix)
                WriteXor(s.Length);
            
            WriteXor(s.ToCharArray());
        }
    }
}