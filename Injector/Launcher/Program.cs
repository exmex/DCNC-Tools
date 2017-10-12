using Launcher.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Launcher
{
    static class Program
    {
        public static void Main()
        {
            if (!File.Exists("DriftCity.exe"))
            {
                Console.WriteLine("Please put the launcher in the DriftCity game directory!");
                Console.ReadKey();
                return;
            }

#if !USE_RES
            if (!File.Exists("DCNCHook.dll"))
            {
                Console.WriteLine("Please put the hook (DCNCHook.dll) in the same directory as launcher!");
                Console.ReadKey();
                return;
            }
#endif

            if (!Utils.IsAdmin())
                Console.WriteLine("Not running as admin, if injection fails the application cannot kill DC!");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "DriftCity.exe",
                Arguments = $"/dev /runsw {Utils.GetRunSw()}"
            };
            Process dcProcess = Process.Start(startInfo);
            if (dcProcess == null) return;
#if USE_RES
            if (Utils.Inject(Resources.DCNCHook))
                return;
#else
            if (Utils.Inject("DCNCHook.dll"))
                return;
#endif

			try
			{
				dcProcess.Kill();
				Process.GetProcessesByName("gameguard.des").First().Kill();
				Process.GetProcessesByName("gamemon.des").First().Kill();
				Process.GetProcessesByName("gamemon64.des").First().Kill();
			}
			catch(Exception)
			{
				Console.WriteLine("There was an error trying to kill the processes!");
				// ignored.
			}
				
            Console.WriteLine("Injecting of DLL Failed.");
            Console.ReadKey();
        }
    }
}
