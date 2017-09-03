using System;
using System.Runtime.InteropServices;
using System.Text;

namespace JLibrary.PortableExecutable
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct IMAGE_SECTION_HEADER
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public byte[] Name;

		public uint VirtualSize;

		public uint VirtualAddress;

		public uint SizeOfRawData;

		public uint PointerToRawData;

		public uint PointerToRelocations;

		public uint PointerToLineNumbers;

		public ushort NumberOfRelocations;

		public ushort NumberOfLineNumbers;

		public uint Characteristics;

		public override string ToString()
		{
			string text = Encoding.UTF8.GetString(this.Name);
			if (text.Contains("\0"))
			{
				text = text.Substring(0, text.IndexOf("\0"));
			}
			return text;
		}
	}
}
