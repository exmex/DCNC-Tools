using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace TdfReader
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length == 1 && File.Exists(args[0]))
            {
                Application.Run(new Form1(args[0]));
            }
            else
                Application.Run(new Form1());
        }
    }
}
