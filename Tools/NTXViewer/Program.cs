using System;
using System.IO;
using System.Windows.Forms;

namespace NTXViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length == 1 && File.Exists(args[0]))
            {
                Application.Run(new Viewer(args[0]));
            }
            else
                Application.Run(new Viewer());
        }
    }
}
