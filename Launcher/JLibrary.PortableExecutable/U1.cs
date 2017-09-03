using System;
using System.Runtime.InteropServices;

namespace JLibrary.PortableExecutable
{
	[Serializable]
	[StructLayout(LayoutKind.Explicit)]
	public struct U1
	{
		[FieldOffset(0)]
		public uint ForwarderString;

		[FieldOffset(0)]
		public uint Function;

		[FieldOffset(0)]
		public uint Ordinal;

		[FieldOffset(0)]
		public uint AddressOfData;
	}
}
