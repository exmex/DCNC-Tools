using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ionic.Zlib;

namespace NayaPack
{
    public class AGTFile
    {
        private const string FileIdentifier = "NayaPack";
        private const int MaxFileChunkSize = 16384;
        
        private long _currentFileInfoOffset = 0x20;
        private long _currentChunkOffset;

        public int FileCount;
        public Dictionary<string, byte[]> Files;

        public void Load(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            using (var ms = new MemoryStream(fileBytes))
            {
                using (var reader = new XorBinaryReader(ms))
                {
                    var identifier = Encoding.UTF8.GetString(reader.ReadBytes(8));
                    if(identifier != FileIdentifier)
                        throw new Exception("Invalid file specified");

                    reader.ReadInt32(); // Unknown
                    reader.ReadInt32(); // Unknown

                    FileCount = reader.ReadInt32();
                    Files = new Dictionary<string, byte[]>(FileCount);
                    
                    for (var i = 0; i < FileCount; i++)
                    {
                        // Seek to our current file info position
                        reader.BaseStream.Seek(_currentFileInfoOffset, SeekOrigin.Begin);
                        
                        // Get the first chunk position
                        _currentChunkOffset = reader.ReadInt32Xor();

                        // Read how many chunks (Max uncompressed size: 16384) this file has
                        var fileChunks = reader.ReadInt32Xor();
                        
                        // Read the total uncompressed file size
                        var fileSize = reader.ReadInt32Xor();
                        
                        // Read filename and add it to our files
                        var currentFileName = reader.ReadStringXor();
                        Files.Add(currentFileName, new byte[fileSize]);
                        
                        // Set our current file info position to next file
                        _currentFileInfoOffset = reader.BaseStream.Position;
                        
                        var currentFileChunkOffset = (fileChunks * 2) + _currentChunkOffset;
                        for (var j = 0; j < fileChunks; j++)
                        {
                            // Seek to the current chunk offset position
                            reader.BaseStream.Seek(_currentChunkOffset, SeekOrigin.Begin);
                            
                            // Read how big this chunk is
                            var compressedSize = reader.ReadInt16Xor();
                            
                            // Set the current chunk offset position to next chunk
                            _currentChunkOffset = reader.BaseStream.Position;
                            
                            // Seek to the chunk files offset
                            reader.BaseStream.Seek(currentFileChunkOffset, SeekOrigin.Begin);
                            
                            // Set file chunk offset to next file chunk
                            currentFileChunkOffset += compressedSize;
                            
                            // Read the compressed chunk
                            var compressedFile = reader.ReadBytesXor(compressedSize);
                            
                            // Decompress the current chunk
                            var decompressedFile = DecompressZLibRaw(compressedFile);
                            
                            // Check if the chunk isn't too big?
                            if (decompressedFile.Length > MaxFileChunkSize)
                                throw new Exception($"Chunk {j} for file {currentFileName} was too big! Corrupted archive?");
                            
                            Array.Copy(decompressedFile, Files[currentFileName], decompressedFile.Length);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Helper function to decompress a byte array
        /// </summary>
        /// <param name="compressed">The byte array to decompress</param>
        /// <returns>The decompressed byte array</returns>
        private static byte[] DecompressZLibRaw(byte[] compressed)
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
        private static byte[] CompressZLibRaw(byte[] uncompressed)
        {
            using (var outputMemoryStream = new MemoryStream()) 
            using (var uncompressedMemoryStream = new MemoryStream())
            {
                uncompressedMemoryStream.Write(uncompressed, 0, uncompressed.Length);
                uncompressedMemoryStream.Position = 0;
                using (var decomp = new ZlibStream(uncompressedMemoryStream, CompressionMode.Compress))
                    decomp.CopyTo(outputMemoryStream);
                
                return outputMemoryStream.ToArray();
            }
        }
    }
}