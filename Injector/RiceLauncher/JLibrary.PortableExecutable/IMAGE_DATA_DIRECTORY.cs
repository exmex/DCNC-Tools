using System;

namespace JLibrary.PortableExecutable
{
	[Serializable]
	public struct IMAGE_DATA_DIRECTORY
	{
		public uint VirtualAddress;

		public uint Size;
	}
}
