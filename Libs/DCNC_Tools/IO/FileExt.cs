using System;
using System.Collections.Generic;
using System.IO;

namespace DCNC_Tools.IO
{
    public static class FileExt
    {
        public static byte[] ReadAllBytesNoLock(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            byte[] oFileBytes;
            using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var numBytesToRead = Convert.ToInt32(fs.Length);
                oFileBytes = new byte[(numBytesToRead)];
                fs.Read(oFileBytes, 0, numBytesToRead);
            }
            return oFileBytes;
        }
        public static string[] ReadAllLinesNoLock(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            List<string> oLines = new List<string>();
            using (var reader = new StreamReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    oLines.Add(line);
                }
            }
            return oLines.ToArray();
        }
    }
}
