using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;

namespace DCNC_Tools.Utils
{
    public static class Compression
    {
        /// <summary>
        /// Helper function to decompress a byte array
        /// </summary>
        /// <param name="compressed">The byte array to decompress</param>
        /// <returns>The decompressed byte array</returns>
        public static byte[] DecompressZLibRaw(byte[] compressed)
        {
            using (var outputMemoryStream = new MemoryStream())
            using (var compressedMemoryStream = new MemoryStream())
            {
                compressedMemoryStream.Write(compressed, 0, compressed.Length);
                compressedMemoryStream.Position = 0;
                using (var decomp = new ZlibStream(compressedMemoryStream, CompressionMode.Decompress))
                    decomp.CopyTo(outputMemoryStream);
                return outputMemoryStream.ToArray();
            }
        }

        /// <summary>
        /// Helper function to compress a byte array
        /// </summary>
        /// <param name="uncompressed">The byte array to compress</param>
        /// <returns>The compressed byte array</returns>
        public static byte[] CompressZLibRaw(byte[] uncompressed)
        {
            using (var outputMemoryStream = new MemoryStream())
            using (var uncompressedMemoryStream = new MemoryStream())
            {
                uncompressedMemoryStream.Write(uncompressed, 0, uncompressed.Length);
                uncompressedMemoryStream.Position = 0;
                using (var decomp = new ZlibStream(uncompressedMemoryStream, CompressionMode.Compress, CompressionLevel.Default))
                    decomp.CopyTo(outputMemoryStream);

                return outputMemoryStream.ToArray();
            }
        }
    }
}
