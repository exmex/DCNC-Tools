using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCNC_Tools.Formats;

namespace DCNC_Tools.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = new AGT();
            if(args.Length != 1)
                file.Load("jp_Init.agt");
            else
                file.Load(args[0]);

            Console.ReadKey();
            foreach (var keyValuePair in file.Files)
            {
                if (!Directory.Exists(Path.GetDirectoryName(keyValuePair.Key)))
                    Directory.CreateDirectory(Path.GetDirectoryName(keyValuePair.Key));
                File.WriteAllBytes(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}
