using System;
using System.IO;
using System.Windows.Forms;

namespace TdfEditor
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Slowing down app.
            //Application.EnableVisualStyles();
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
