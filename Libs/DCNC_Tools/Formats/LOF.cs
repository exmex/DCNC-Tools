using System.Collections.Generic;
using System.IO;
using DCNC_Tools.IO;

namespace DCNC_Tools.Formats
{
    public class LOF
    {
        public readonly List<FILE> Files = new List<FILE>();

        public LOF(string fileName)
        {
            Load(fileName);
        }

        private void Load(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            using (var ms = new MemoryStream(fileBytes))
            {
                using (var reader = new BinaryReaderExt(ms))
                {
                    reader.ReadBytes(16);
                    var fileCount = reader.ReadInt32();
                    reader.ReadInt32();
                    for (var i = 0; i < fileCount; i++)
                    {
                        var file = new FILE();
                        file.Index = reader.ReadInt32();
                        reader.ReadBytes(21);
                        file.FileName = reader.ReadAscii();
                        file.FileName =
                            file.FileName.Substring(0, file.FileName.IndexOf('\0')); // Otherwise windows will crap out
                        reader.ReadBytes(12);
                        var filePos = reader.ReadInt32();
                        var fileLength = reader.ReadInt32();

                        var pos = reader.BaseStream.Position;

                        reader.BaseStream.Seek(filePos, SeekOrigin.Begin);
                        file.File = reader.ReadBytes(fileLength);
                        reader.BaseStream.Seek(pos, SeekOrigin.Begin);

                        Files.Add(file);
                    }
                }
            }
        }

        public struct FILE
        {
            public int Index;
            public string FileName;
            public byte[] File;
        }
    }
}