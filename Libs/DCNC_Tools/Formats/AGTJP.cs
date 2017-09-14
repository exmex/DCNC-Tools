using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using DCNC_Tools.IO;
using DCNC_Tools.Utils;
using DCNC_Tools.Utils.DDSReader;
using Ionic.Zlib;

namespace DCNC_Tools.Formats
{
    public class AGTJP
    {
        private const string FileIdentifier = "NayaPack";
        private const int MaxFileChunkSize = 16384;
        private const int StartFileInfoOffset = 0x20;
        
        private int _currentFileInfoOffset = StartFileInfoOffset;
        private int _currentChunkOffset;

        public int FileCount;
        public Dictionary<string, byte[]> Files;

        public short VersionMajor;
        public short VersionMinor;

        public static readonly byte[] XorKey = {
            0x01, 0x05, 0x06, 0x02, 0x04, 0x03, 0x07, 0x08, 0x01, 0x05, 0x06, 0x0F, 0x04, 0x03, 0x07, 0x0C, 0x31, 0x85,
            0x76, 0x39, 0x34, 0x3D, 0x30, 0xE8, 0x67, 0x36, 0x36, 0x32, 0x3E, 0x33, 0x34, 0x3B, 0x11, 0x15, 0x16, 0x16,
            0x14, 0x13, 0x1D, 0x18, 0x11, 0x03, 0x06, 0x0C, 0x04, 0x03, 0x06, 0x08, 0x2E, 0x55, 0x26, 0x23, 0x2A, 0x23,
            0x2E, 0x28, 0x21, 0x21, 0x26, 0x27, 0x2E, 0x00, 0x2D, 0x2D, 0xCF, 0xA5, 0x06, 0x02, 0x04, 0x0F, 0x07, 0x18,
            0xE1, 0x15, 0x36, 0x18, 0x60, 0x13, 0x1A, 0x19, 0x11, 0x15, 0x16, 0x10, 0x12, 0x13, 0x17, 0x38, 0xF1, 0x25
        };
        
        /// <summary>
        /// Loads the specified AGT/NayaPack file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <exception cref="InvalidFileHeaderException">If the fileheader is invalid</exception>
        /// <exception cref="System.FormatException">If file inside the AGT was corrupt</exception>
        public void Load(string fileName)
        {
            Load(fileName, XorKey);
        }

        /// <summary>
        /// Loads the specified AGT/NayaPack file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <exception cref="InvalidFileHeaderException">If the fileheader is invalid</exception>
        /// <exception cref="System.FormatException">If file inside the AGT was corrupt</exception>
        public void Load(string fileName, byte[] xorKey)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            using (var ms = new MemoryStream(fileBytes))
            {
                using (var reader = new XorBinaryReader(ms, xorKey))
                {
                    var identifier = Encoding.UTF8.GetString(reader.ReadBytes(8));
                    if(identifier != FileIdentifier)
                        throw new InvalidFileHeaderException();

                    reader.ReadInt32(); // Unknown
                    VersionMajor = reader.ReadInt16();
                    VersionMinor = reader.ReadInt16();

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
                        _currentFileInfoOffset = (int)reader.BaseStream.Position;

                        var currentFileChunkOffset = (fileChunks * 2) + _currentChunkOffset;
                        var totalChunksSizeRead = 0;
                        for (var j = 0; j < fileChunks; j++)
                        {
                            // Seek to the current chunk offset position
                            reader.BaseStream.Seek(_currentChunkOffset, SeekOrigin.Begin);

                            // Read how big this chunk is
                            var compressedSize = reader.ReadInt16Xor();
                            
                            // Set the current chunk offset position to next chunk
                            _currentChunkOffset = (int)reader.BaseStream.Position;
                            
                            // Seek to the chunk files offset
                            reader.BaseStream.Seek(currentFileChunkOffset, SeekOrigin.Begin);
                            
                            // Set file chunk offset to next file chunk
                            currentFileChunkOffset += compressedSize;
                            
                            // Read the compressed chunk
                            var compressedFile = reader.ReadBytesXor(compressedSize);

                            // Weird try catch hack for uncompressed data.
                            if (compressedFile[0] == 0x78 && compressedFile[1] >= 0x01 && compressedFile[1] <= 0xDA)
                            {
                                try
                                {
                                    // Decompress the current chunk
                                    var decompressedFile = DecompressZLibRaw(compressedFile);
                                    // Check if the chunk isn't too big?
                                    if (decompressedFile.Length > MaxFileChunkSize)
                                        throw new FormatException(
                                            $"Chunk {j} for file {currentFileName} was too big! Corrupted archive?");
                                    Array.Copy(decompressedFile, 0, Files[currentFileName], totalChunksSizeRead, decompressedFile.Length);
                                    totalChunksSizeRead += decompressedFile.Length;
                                }
                                catch (Exception)
                                {
                                    // Ionic.Zlib had issues with non-compressed files with ZLIB header.
                                    Debug.WriteLine(@"{0} File is not compressed but tried to decompress (Header: {1:X} {2:X})", currentFileName, compressedFile[0], compressedFile[1]);
                                    Console.WriteLine(@"{0} File is not compressed but tried to decompress (Header: {1:X} {2:X})", currentFileName, compressedFile[0], compressedFile[1]);

                                    // Weird hack for JP!
                                    if (Files[currentFileName].Length < totalChunksSizeRead+compressedFile.Length)
                                    {
                                        var arr = Files[currentFileName];
                                        Array.Resize(ref arr, totalChunksSizeRead + compressedFile.Length);
                                        Files[currentFileName] = arr;
                                    }
                                    Array.Copy(compressedFile, 0, Files[currentFileName], totalChunksSizeRead, compressedFile.Length);
                                    totalChunksSizeRead += compressedFile.Length;
                                }
                            }
                            else
                            {
                                // File is uncompressed. Just read the "compressedFile" in.
                                Array.Copy(compressedFile, 0, Files[currentFileName], totalChunksSizeRead, compressedFile.Length);
                                totalChunksSizeRead += compressedFile.Length;
                            }
                        }
                        Debug.WriteLineIf(Files[currentFileName].Length != fileSize, $"Reported: {fileSize}, Actua: {Files[currentFileName].Length}");
                        /*if(Files[currentFileName].Length != fileSize)
                            throw new FormatException($"File {currentFileName} corrupt?");*/
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