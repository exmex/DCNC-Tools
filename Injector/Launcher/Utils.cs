using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using InjectionLibrary;
using JLibrary.PortableExecutable;
using Launcher.Properties;

namespace Launcher
{
    public static class Utils
    {
        [DllImport("winmm.dll", EntryPoint = "timeGetTime")]
        public static extern uint GetTime();

        public static string GetRunSw()
        {
            var num = GetTime();
            var s = (num - num % 300000u).ToString();
            var value = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(s));
            return BitConverter.ToString(value).Replace("-", "").ToLower();
        }

        public static bool Inject(byte[] file)
        {
            Process[] processesByName;
            do
            {
                Thread.Sleep(1);
                processesByName = Process.GetProcessesByName("driftcity");
            } while (processesByName.Length == 0);
            var injectionMethod = InjectionMethod.Create(InjectionMethodType.ManualMap);
            IntPtr value;
            using (var portableExecutable = new PortableExecutable(file))
            {
                value = injectionMethod.Inject(portableExecutable, processesByName[0].Id);
            }
            return value != IntPtr.Zero;
        }

        public static bool Inject(string fileName)
        {
            Process[] processesByName;
            do
            {
                Thread.Sleep(1);
                processesByName = Process.GetProcessesByName("driftcity");
            } while (processesByName.Length == 0);
            var injectionMethod = InjectionMethod.Create(InjectionMethodType.ManualMap);
            IntPtr value;
            using (var portableExecutable = new PortableExecutable(fileName))
            {
                value = injectionMethod.Inject(portableExecutable, processesByName[0].Id);
            }
            return value != IntPtr.Zero;
        }

        public static bool IsAdmin()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}