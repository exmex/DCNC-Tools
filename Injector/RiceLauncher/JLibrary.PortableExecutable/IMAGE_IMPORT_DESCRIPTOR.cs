using System;
using System.Runtime.InteropServices;

namespace JLibrary.PortableExecutable
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct IMAGE_IMPORT_DESCRIPTOR
	{
		public uint OriginalFirstThunk;

		public uint TimeDateStamp;

		public uint ForwarderChain;

		public uint Name;

		public uint FirstThunkPtr;
	}
}
