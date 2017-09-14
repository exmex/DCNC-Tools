using JLibrary.PortableExecutable;
using JLibrary.Tools;
using JLibrary.Win32;
using System;

namespace InjectionLibrary
{
	public abstract class InjectionMethod : ErrorBase
	{
		public InjectionMethodType Type
		{
			get;
			protected set;
		}

		public abstract IntPtr Inject(string dllPath, IntPtr hProcess);

		public virtual IntPtr Inject(string dllPath, int processId)
		{
			this.ClearErrors();
			IntPtr intPtr = WinAPI.OpenProcess(1082u, false, processId);
			IntPtr result = this.Inject(dllPath, intPtr);
			WinAPI.CloseHandle(intPtr);
			return result;
		}

		public abstract IntPtr[] InjectAll(string[] dllPaths, IntPtr hProcess);

		public virtual IntPtr[] InjectAll(string[] dllPaths, int processId)
		{
			this.ClearErrors();
			IntPtr intPtr = WinAPI.OpenProcess(1082u, false, processId);
			IntPtr[] result = this.InjectAll(dllPaths, intPtr);
			WinAPI.CloseHandle(intPtr);
			return result;
		}

		public abstract IntPtr Inject(PortableExecutable image, IntPtr hProcess);

		public virtual IntPtr Inject(PortableExecutable image, int processId)
		{
			this.ClearErrors();
			IntPtr intPtr = WinAPI.OpenProcess(1082u, false, processId);
			IntPtr result = this.Inject(image, intPtr);
			WinAPI.CloseHandle(intPtr);
			return result;
		}

		public abstract IntPtr[] InjectAll(PortableExecutable[] images, IntPtr hProcess);

		public virtual IntPtr[] InjectAll(PortableExecutable[] images, int processId)
		{
			this.ClearErrors();
			IntPtr intPtr = WinAPI.OpenProcess(1082u, false, processId);
			IntPtr[] result = this.InjectAll(images, intPtr);
			WinAPI.CloseHandle(intPtr);
			return result;
		}

		public abstract bool Unload(IntPtr hModule, IntPtr hProcess);

		public virtual bool Unload(IntPtr hModule, int processId)
		{
			this.ClearErrors();
			IntPtr intPtr = WinAPI.OpenProcess(1082u, false, processId);
			bool result = this.Unload(hModule, intPtr);
			WinAPI.CloseHandle(intPtr);
			return result;
		}

		public abstract bool[] UnloadAll(IntPtr[] hModules, IntPtr hProcess);

		public virtual bool[] UnloadAll(IntPtr[] hModules, int processId)
		{
			this.ClearErrors();
			IntPtr intPtr = WinAPI.OpenProcess(1082u, false, processId);
			bool[] result = this.UnloadAll(hModules, intPtr);
			WinAPI.CloseHandle(intPtr);
			return result;
		}

		public static InjectionMethod Create(InjectionMethodType type)
		{
			InjectionMethod injectionMethod;
			switch (type)
			{
			case InjectionMethodType.Standard:
				injectionMethod = new CRTInjection();
				break;
			case InjectionMethodType.ThreadHijack:
				injectionMethod = new ThreadHijack();
				break;
			case InjectionMethodType.ManualMap:
				injectionMethod = new ManualMap();
				break;
			default:
				return null;
			}
			if (injectionMethod != null)
			{
				injectionMethod.Type = type;
			}
			return injectionMethod;
		}
	}
}
