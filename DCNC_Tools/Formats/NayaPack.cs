using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DCNC_Tools.IO;
using DCNC_Tools.Utils;
using Ionic.Zlib;

namespace DCNC_Tools.Formats
{
    public class NayaPack
    {
        private const string FileIdentifier = "NayaPack";
        private const int MaxFileChunkSize = 16384;
        private const int StartFileInfoOffset = 0x20;
        
        private long _currentFileInfoOffset = StartFileInfoOffset;
        private long _currentChunkOffset;

        public int FileCount;
        public Dictionary<string, byte[]> Files;

        private static readonly byte[] XorKey = {
            0x01, 0x05, 0x06, 0x02, 0x04, 0x03, 0x07, 0x08, 0x01, 0x05, 0x06, 0x0f, 0x04, 0x03, 0x07, 0x0c, 0x31, 0x85,
            0x76, 0x39, 0x34, 0x3d, 0x30, 0xe8, 0x67, 0x36, 0x36, 0x32, 0x3e, 0x33, 0x34, 0x3b, 0x11, 0x15, 0x16, 0x16,
            0x14, 0x13, 0x1d, 0x18, 0x11, 0x03, 0x06, 0x0c, 0x04, 0x03, 0x06, 0x08, 0x2e, 0x55, 0x26, 0x23, 0x2a, 0x23,
            0x2e, 0x28, 0x21, 0x21, 0x26, 0x27, 0x2e, 0x00, 0x2d, 0x2d, 0xcf, 0xa5, 0x06, 0x02, 0x04, 0x0f, 0x07, 0x18,
            0xe1, 0x15, 0x36, 0x18, 0x60, 0x13, 0x1a, 0x19, 0x11, 0x15, 0x16, 0x10, 0x12, 0x13, 0x17, 0x38, 0xf1, 0x25
        };

#if DEBUG
        public void Save(string fileName)
        {
            _currentFileInfoOffset = StartFileInfoOffset;
            
            using (var fs = File.Open(fileName, FileMode.Create))
            {
                using (var writer = new XorBinaryWriter(fs, XorKey))
                {
                    _currentChunkOffset = _currentFileInfoOffset + FileCount * 3;
                    writer.Write(FileIdentifier.ToCharArray());
                    
                    writer.Write(0);
                    writer.Write(65537);
                    
                    writer.Write(FileCount);

                    var totalStringLength = 0;
                    foreach (var file in Files)
                        totalStringLength += file.Key.Length;
                    
                    _currentChunkOffset = _currentFileInfoOffset + FileCount*16 + totalStringLength;
                    
                    foreach (var file in Files)
                    {
                        writer.BaseStream.Seek(_currentFileInfoOffset, SeekOrigin.Begin);
                        
                        writer.Write(_currentChunkOffset);
                        
                        var fileChunks = (int)Math.Ceiling((double)file.Value.Length / MaxFileChunkSize);
                        
                        // Write how many chunks this file uses
                        writer.Write(fileChunks);
                        
                        // Write total uncompressed filesize
                        writer.Write(file.Value.Length);
                        
                        // Write filename
                        writer.Write(file.Key);
                        
                        // Set our current file info position to next file
                        _currentFileInfoOffset = writer.BaseStream.Position;
                        
                        var currentFileChunkOffset = (fileChunks * 2) + _currentChunkOffset;

                        var bytesLeft = file.Value;
                        for (var j = 0; j < fileChunks; j++)
                        {
                            // Seek to the current chunk offset position
                            writer.BaseStream.Seek(_currentChunkOffset, SeekOrigin.Begin);

                            byte[] chunkBytes;
                            if (bytesLeft.Length >= MaxFileChunkSize)
                            {
                                chunkBytes = new byte[MaxFileChunkSize];
                                // Copy chunk from bytesleft
                                Array.Copy(bytesLeft, 0, chunkBytes, 0, MaxFileChunkSize);

                                // Remove chunk from bytesleft
                                Array.Copy(bytesLeft, MaxFileChunkSize, bytesLeft, 0,
                                    bytesLeft.Length - MaxFileChunkSize);
                            }
                            else
                            {
                                chunkBytes = new byte[bytesLeft.Length];
                                Array.Copy(bytesLeft, chunkBytes, bytesLeft.Length);
                            }
                            var compressedFile = CompressZLibRaw(chunkBytes);
                            
                            writer.Write((short)compressedFile.Length);
                            
                            // Set the current chunk offset position to next chunk
                            _currentChunkOffset = writer.BaseStream.Position;
                            
                            // Seek to the chunk files offset
                            writer.BaseStream.Seek(currentFileChunkOffset, SeekOrigin.Begin);
                            
                            currentFileChunkOffset += compressedFile.Length;
                            
                            writer.Write(compressedFile);
                        }
                    }
                }
            }
        }
        /*
        Chunk 0:
                Compressed: 2049
                Uncompressed:16384
        Chunk 1:
                Compressed: 1988
                Uncompressed:16384
        Chunk 2:
                Compressed: 2033
                Uncompressed:16384
        Chunk 3:
                Compressed: 747
                Uncompressed:16384
        Chunk 4:
                Compressed: 23
                Uncompressed:350
        */
#endif

        public void UnExor(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            using(var writer = new BinaryWriter(File.Open(fileName+".bak", FileMode.Create)))
            using (var ms = new MemoryStream(fileBytes))
            {
                using (var reader = new XorBinaryReader(ms, XorKey))
                {
                    var identifier = Encoding.UTF8.GetString(reader.ReadBytes(8));
                    writer.Write(identifier.ToCharArray());
                    
                    if(identifier != FileIdentifier)
                        throw new Exception("Invalid file specified");

                    writer.Write(reader.ReadInt32()); // Unknown
                    writer.Write(reader.ReadInt32()); // Unknown (Always 65537?)

                    FileCount = reader.ReadInt32();
                    writer.Write(FileCount);
                    Files = new Dictionary<string, byte[]>(FileCount);
                    
                    for (var i = 0; i < FileCount; i++)
                    {
                        // Seek to our current file info position
                        reader.BaseStream.Seek(_currentFileInfoOffset, SeekOrigin.Begin);
                        
                        // Get the first chunk position
                        _currentChunkOffset = reader.ReadInt32Xor();
                        writer.Write(_currentChunkOffset);

                        // Read how many chunks (Max uncompressed size: 16384) this file has
                        var fileChunks = reader.ReadInt32Xor();
                        writer.Write(fileChunks);
                        
                        // Read the total uncompressed file size
                        var fileSize = reader.ReadInt32Xor();
                        writer.Write(fileSize);
                        
                        // Read filename and add it to our files
                        var currentFileName = reader.ReadStringXor();
                        writer.Write(currentFileName);
                        
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
                            writer.Write(compressedSize);
                            
                            // Set the current chunk offset position to next chunk
                            _currentChunkOffset = reader.BaseStream.Position;
                            
                            // Seek to the chunk files offset
                            reader.BaseStream.Seek(currentFileChunkOffset, SeekOrigin.Begin);
                            
                            // Set file chunk offset to next file chunk
                            currentFileChunkOffset += compressedSize;
                            
                            // Read the compressed chunk
                            var compressedFile = reader.ReadBytesXor(compressedSize);
                            writer.Write(compressedFile);
                            
                            // Check if the chunk isn't too big?
                            if (compressedFile.Length > MaxFileChunkSize)
                                throw new Exception($"Chunk {j} for file {currentFileName} was too big! Corrupted archive?");
                            
                            // Decompress the current chunk
                            var decompressedFile = DecompressZLibRaw(compressedFile);
                            
                            Array.Copy(decompressedFile, Files[currentFileName], decompressedFile.Length);
                        }
                        if(Files[currentFileName].Length != fileSize)
                            throw new Exception($"File {currentFileName} corrupt?");
                    }
                }
            }
        }

        public void Load(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            using (var ms = new MemoryStream(fileBytes))
            {
                using (var reader = new XorBinaryReader(ms, XorKey))
                {
                    var identifier = Encoding.UTF8.GetString(reader.ReadBytes(8));
                    if(identifier != FileIdentifier)
                        throw new Exception("Invalid file specified");

                    reader.ReadInt32(); // Unknown
                    reader.ReadInt32(); // Unknown (Always 65537?)

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
                        if(Files[currentFileName].Length != fileSize)
                            throw new Exception($"File {currentFileName} corrupt?");
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
                using (var decomp = new ZlibStream(uncompressedMemoryStream, CompressionMode.Compress, CompressionLevel.Default))
                    decomp.CopyTo(outputMemoryStream);
                
                return outputMemoryStream.ToArray();
            }
        }
    }
}