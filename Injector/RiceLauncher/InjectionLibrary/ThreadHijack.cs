using JLibrary.Win32;
using System;
using System.Diagnostics;
using System.Threading;

namespace InjectionLibrary
{
	internal class ThreadHijack : StandardInjectionMethod
	{
		private static readonly byte[] REDIRECT_STUB = new byte[]
		{
			156,
			96,
			232,
			0,
			0,
			0,
			0,
			97,
			157,
			233,
			0,
			0,
			0,
			0
		};

		public override IntPtr Inject(string dllPath, IntPtr hProcess)
		{
			this.ClearErrors();
			IntPtr[] array = this.InjectAll(new string[]
			{
				dllPath
			}, hProcess);
			if (array != null && array[0].IsNull() && this.GetLastError() == null)
			{
				this.SetLastError(new Exception("Module's entry point function reported a failure"));
			}
			if (array == null || array.Length <= 0)
			{
				return IntPtr.Zero;
			}
			return array[0];
		}

		public override IntPtr[] InjectAll(string[] dllPaths, IntPtr hProcess)
		{
			this.ClearErrors();
			IntPtr[] result;
			try
			{
				if (hProcess.IsNull() || hProcess.Compare(-1L))
				{
					throw new ArgumentException("Invalid process handle.", "hProcess");
				}
				int processId = WinAPI.GetProcessId(hProcess);
				if (processId == 0)
				{
					throw new ArgumentException("Provided handle doesn't have sufficient permissions to inject", "hProcess");
				}
				Process processById = Process.GetProcessById(processId);
				if (processById.Threads.Count == 0)
				{
					throw new Exception("Target process has no targetable threads to hijack.");
				}
				ProcessThread processThread = ThreadHijack.SelectOptimalThread(processById);
				IntPtr intPtr = WinAPI.OpenThread(26u, false, processThread.Id);
				if (intPtr.IsNull() || intPtr.Compare(-1L))
				{
					throw new Exception("Unable to obtain a handle for the remote thread.");
				}
				IntPtr zero = IntPtr.Zero;
				IntPtr zero2 = IntPtr.Zero;
				IntPtr intPtr2 = this.CreateMultiLoadStub(dllPaths, hProcess, out zero, 1u);
				IntPtr[] array = null;
				if (!intPtr2.IsNull())
				{
					if (WinAPI.SuspendThread(intPtr) == 4294967295u)
					{
						throw new Exception("Unable to suspend the remote thread");
					}
					try
					{
						uint num = 0u;
						WinAPI.CONTEXT cONTEXT = default(WinAPI.CONTEXT);
						cONTEXT.ContextFlags = 65537u;
						if (!WinAPI.GetThreadContext(intPtr, ref cONTEXT))
						{
							throw new InvalidOperationException("Cannot get the remote thread's context");
						}
						byte[] rEDIRECT_STUB = ThreadHijack.REDIRECT_STUB;
						IntPtr intPtr3 = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint)rEDIRECT_STUB.Length, 12288, 64);
						if (intPtr3.IsNull())
						{
							throw new InvalidOperationException("Unable to allocate memory in the remote process.");
						}
						BitConverter.GetBytes(intPtr2.Subtract(intPtr3.Add(7L)).ToInt32()).CopyTo(rEDIRECT_STUB, 3);
						BitConverter.GetBytes((uint)((ulong)cONTEXT.Eip - (ulong)((long)intPtr3.Add((long)rEDIRECT_STUB.Length).ToInt32()))).CopyTo(rEDIRECT_STUB, rEDIRECT_STUB.Length - 4);
						if (!WinAPI.WriteProcessMemory(hProcess, intPtr3, rEDIRECT_STUB, rEDIRECT_STUB.Length, out num) || num != (uint)rEDIRECT_STUB.Length)
						{
							throw new InvalidOperationException("Unable to write stub to the remote process.");
						}
						cONTEXT.Eip = (uint)intPtr3.ToInt32();
						WinAPI.SetThreadContext(intPtr, ref cONTEXT);
					}
					catch (Exception lastError)
					{
						this.SetLastError(lastError);
						array = null;
						WinAPI.VirtualFreeEx(hProcess, zero, 0, 32768);
						WinAPI.VirtualFreeEx(hProcess, intPtr2, 0, 32768);
						WinAPI.VirtualFreeEx(hProcess, zero2, 0, 32768);
					}
					WinAPI.ResumeThread(intPtr);
					if (this.GetLastError() == null)
					{
						Thread.Sleep(100);
						array = new IntPtr[dllPaths.Length];
						byte[] array2 = WinAPI.ReadRemoteMemory(hProcess, zero, (uint)((uint)dllPaths.Length << 2));
						if (array2 != null)
						{
							for (int i = 0; i < array.Length; i++)
							{
								array[i] = Win32Ptr.Create((long)BitConverter.ToInt32(array2, i << 2));
							}
						}
					}
					WinAPI.CloseHandle(intPtr);
				}
				result = array;
			}
			catch (Exception lastError2)
			{
				this.SetLastError(lastError2);
				result = null;
			}
			return result;
		}

		private static ProcessThread SelectOptimalThread(Process target)
		{
			return target.Threads[0];
		}
	}
}
