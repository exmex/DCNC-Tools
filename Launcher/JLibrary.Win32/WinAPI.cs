using System;
using System.Runtime.InteropServices;
using System.Text;

namespace JLibrary.Win32
{
	public static class WinAPI
	{
		public struct FLOATING_SAVE_AREA
		{
			public uint ControlWord;

			public uint StatusWord;

			public uint TagWord;

			public uint ErrorOffset;

			public uint ErrorSelector;

			public uint DataOffset;

			public uint DataSelector;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
			public byte[] RegisterArea;

			public uint Cr0NpxState;
		}

		public struct CONTEXT
		{
			public uint ContextFlags;

			public uint Dr0;

			public uint Dr1;

			public uint Dr2;

			public uint Dr3;

			public uint Dr6;

			public uint Dr7;

			public WinAPI.FLOATING_SAVE_AREA FloatSave;

			public uint SegGs;

			public uint SegFs;

			public uint SegEs;

			public uint SegDs;

			public uint Edi;

			public uint Esi;

			public uint Ebx;

			public uint Edx;

			public uint Ecx;

			public uint Eax;

			public uint Ebp;

			public uint Eip;

			public uint SegCs;

			public uint EFlags;

			public uint Esp;

			public uint SegSs;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
			public byte[] ExtendedRegisters;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, int dwThreadId);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr handle);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, int flAllocationType, int flProtect);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int dwFreeType);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint flOldProtect);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpAddress, byte[] lpBuffer, int dwSize, out uint lpNumberOfBytesRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out uint lpNumberOfBytesRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr CreateRemoteThread(IntPtr hProcess, int lpThreadAttributes, int dwStackSize, IntPtr lpStartAddress, uint lpParameter, int dwCreationFlags, int lpThreadId);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetModuleHandleA(string lpModuleName);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, uint lpProcName);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint WaitForSingleObject(IntPtr hObject, int dwTimeout);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetThreadContext(IntPtr hThread, ref WinAPI.CONTEXT pContext);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetThreadContext(IntPtr hThread, ref WinAPI.CONTEXT pContext);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint SuspendThread(IntPtr hThread);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint ResumeThread(IntPtr hThread);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int GetProcessId(IntPtr hProcess);

		public static uint GetLastErrorEx(IntPtr hProcess)
		{
			IntPtr procAddress = WinAPI.GetProcAddress(WinAPI.GetModuleHandleA("kernel32.dll"), "GetLastError");
			return WinAPI.RunThread(hProcess, procAddress, 0u, 1000);
		}

		public static byte[] ReadRemoteMemory(IntPtr hProc, IntPtr address, uint len)
		{
			byte[] array = new byte[len];
			uint num = 0u;
			if (!WinAPI.ReadProcessMemory(hProc, address, array, array.Length, out num) || num != len)
			{
				array = null;
			}
			return array;
		}

		public static uint RunThread(IntPtr hProcess, IntPtr lpStartAddress, uint lpParam, int timeout = 1000)
		{
			uint result = 4294967295u;
			IntPtr intPtr = WinAPI.CreateRemoteThread(hProcess, 0, 0, lpStartAddress, lpParam, 0, 0);
			if (intPtr != IntPtr.Zero && (ulong)WinAPI.WaitForSingleObject(intPtr, timeout) == 0uL)
			{
				WinAPI.GetExitCodeThread(intPtr, out result);
			}
			return result;
		}

		public static IntPtr ReadRemotePointer(IntPtr hProcess, IntPtr pData)
		{
			IntPtr zero = IntPtr.Zero;
			byte[] value;
			if (!hProcess.IsNull() && !pData.IsNull() && (value = WinAPI.ReadRemoteMemory(hProcess, pData, (uint)IntPtr.Size)) != null)
			{
				zero = new IntPtr(BitConverter.ToInt32(value, 0));
			}
			return zero;
		}

		public static IntPtr GetModuleHandleEx(IntPtr hProcess, string lpModuleName)
		{
			IntPtr procAddress = WinAPI.GetProcAddress(WinAPI.GetModuleHandleA("kernel32.dll"), "GetModuleHandleW");
			IntPtr result = IntPtr.Zero;
			if (!procAddress.IsNull())
			{
				IntPtr intPtr = WinAPI.CreateRemotePointer(hProcess, Encoding.Unicode.GetBytes(lpModuleName + "\0"), 4);
				if (!intPtr.IsNull())
				{
					result = Win32Ptr.Create((long)((ulong)WinAPI.RunThread(hProcess, procAddress, (uint)intPtr.ToInt32(), 1000)));
					WinAPI.VirtualFreeEx(hProcess, intPtr, 0, 32768);
				}
			}
			return result;
		}

		public static IntPtr CreateRemotePointer(IntPtr hProcess, byte[] pData, int flProtect)
		{
			IntPtr intPtr = IntPtr.Zero;
			if (pData != null && hProcess != IntPtr.Zero)
			{
				intPtr = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint)pData.Length, 12288, flProtect);
				uint num = 0u;
				if ((intPtr == IntPtr.Zero || !WinAPI.WriteProcessMemory(hProcess, intPtr, pData, pData.Length, out num) || (ulong)num != (ulong)((long)pData.Length)) && intPtr != IntPtr.Zero)
				{
					WinAPI.VirtualFreeEx(hProcess, intPtr, 0, 32768);
					intPtr = IntPtr.Zero;
				}
			}
			return intPtr;
		}

		public static IntPtr GetProcAddressEx(IntPtr hProc, IntPtr hModule, object lpProcName)
		{
			IntPtr result = IntPtr.Zero;
			byte[] array = WinAPI.ReadRemoteMemory(hProc, hModule, 64u);
			if (array != null && BitConverter.ToUInt16(array, 0) == 23117)
			{
				uint num = BitConverter.ToUInt32(array, 60);
				if (num > 0u)
				{
					byte[] array2 = WinAPI.ReadRemoteMemory(hProc, hModule.Add((long)((ulong)num)), 264u);
					if (array2 != null && BitConverter.ToUInt32(array2, 0) == 17744u)
					{
						uint num2 = BitConverter.ToUInt32(array2, 120);
						uint num3 = BitConverter.ToUInt32(array2, 124);
						if (num2 > 0u && num3 > 0u)
						{
							byte[] array3 = WinAPI.ReadRemoteMemory(hProc, hModule.Add((long)((ulong)num2)), 40u);
							uint num4 = BitConverter.ToUInt32(array3, 28);
							uint num5 = BitConverter.ToUInt32(array3, 36);
							uint num6 = BitConverter.ToUInt32(array3, 20);
							int num7 = -1;
							if (num4 > 0u && num5 > 0u)
							{
								if (lpProcName.GetType().Equals(typeof(string)))
								{
									int num8 = WinAPI.SearchExports(hProc, hModule, array3, (string)lpProcName);
									if (num8 > -1)
									{
										byte[] array4 = WinAPI.ReadRemoteMemory(hProc, hModule.Add((long)((ulong)num5 + (ulong)((long)((long)num8 << 1)))), 2u);
										num7 = ((array4 == null) ? -1 : ((int)BitConverter.ToUInt16(array4, 0)));
									}
								}
								else if (lpProcName.GetType().Equals(typeof(short)) || lpProcName.GetType().Equals(typeof(ushort)))
								{
									num7 = int.Parse(lpProcName.ToString());
								}
								if (num7 > -1 && (long)num7 < (long)((ulong)num6))
								{
									byte[] array5 = WinAPI.ReadRemoteMemory(hProc, hModule.Add((long)((ulong)num4 + (ulong)((long)((long)num7 << 2)))), 4u);
									if (array5 != null)
									{
										uint num9 = BitConverter.ToUInt32(array5, 0);
										if (num9 >= num2 && num9 < num2 + num3)
										{
											string text = WinAPI.ReadRemoteString(hProc, hModule.Add((long)((ulong)num9)), null);
											if (!string.IsNullOrEmpty(text) && text.Contains("."))
											{
												result = WinAPI.GetProcAddressEx(hProc, WinAPI.GetModuleHandleEx(hProc, text.Split(new char[]
												{
													'.'
												})[0]), text.Split(new char[]
												{
													'.'
												})[1]);
											}
										}
										else
										{
											result = hModule.Add((long)((ulong)num9));
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		private static int SearchExports(IntPtr hProcess, IntPtr hModule, byte[] exports, string name)
		{
			uint num = BitConverter.ToUInt32(exports, 24);
			uint num2 = BitConverter.ToUInt32(exports, 32);
			int num3 = -1;
			if (num > 0u && num2 > 0u)
			{
				byte[] array = WinAPI.ReadRemoteMemory(hProcess, hModule.Add((long)((ulong)num2)), num << 2);
				if (array != null)
				{
					uint[] array2 = new uint[num];
					for (int i = 0; i < array2.Length; i++)
					{
						array2[i] = BitConverter.ToUInt32(array, i << 2);
					}
					int num4 = 0;
					int num5 = array2.Length - 1;
					string text = string.Empty;
					while (num4 >= 0 && num4 <= num5 && num3 == -1)
					{
						int num6 = (num4 + num5) / 2;
						text = WinAPI.ReadRemoteString(hProcess, hModule.Add((long)((ulong)array2[num6])), null);
						if (text.Equals(name))
						{
							num3 = num6;
						}
						else if (string.CompareOrdinal(text, name) < 0)
						{
							num4 = num6 - 1;
						}
						else
						{
							num5 = num6 + 1;
						}
					}
				}
			}
			return num3;
		}

		public static string ReadRemoteString(IntPtr hProcess, IntPtr lpAddress, Encoding encoding = null)
		{
			if (encoding == null)
			{
				encoding = Encoding.ASCII;
			}
			StringBuilder stringBuilder = new StringBuilder();
			byte[] array = new byte[256];
			uint num = 0u;
			int num2 = -1;
			while (num2 < 0 && WinAPI.ReadProcessMemory(hProcess, lpAddress, array, array.Length, out num) && num > 0u)
			{
				lpAddress = lpAddress.Add((long)((ulong)num));
				int length = stringBuilder.Length;
				stringBuilder.Append(encoding.GetString(array, 0, (int)num));
				num2 = stringBuilder.ToString().IndexOf('\0', length);
			}
			return stringBuilder.ToString().Substring(0, num2);
		}
	}
}
