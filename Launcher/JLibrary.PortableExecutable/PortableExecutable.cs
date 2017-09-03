using JLibrary.Tools;
using System;
using System.Collections.Generic;
using System.IO;

namespace JLibrary.PortableExecutable
{
	[Serializable]
	public class PortableExecutable : MemoryIterator
	{
		public IMAGE_NT_HEADER32 NTHeader
		{
			get;
			private set;
		}

		public IMAGE_DOS_HEADER DOSHeader
		{
			get;
			private set;
		}

		public string FileLocation
		{
			get;
			private set;
		}

		public PortableExecutable(string path) : this(File.ReadAllBytes(path))
		{
			this.FileLocation = path;
		}

		public PortableExecutable(byte[] data) : base(data)
		{
			string text = string.Empty;
			IMAGE_NT_HEADER32 nTHeader = default(IMAGE_NT_HEADER32);
			IMAGE_DOS_HEADER dOSHeader = default(IMAGE_DOS_HEADER);
			if (base.Read<IMAGE_DOS_HEADER>(out dOSHeader) && dOSHeader.e_magic == 23117)
			{
				if (base.Read<IMAGE_NT_HEADER32>((long)((ulong)dOSHeader.e_lfanew), SeekOrigin.Begin, out nTHeader) && (long)nTHeader.Signature == 17744L)
				{
					if (nTHeader.OptionalHeader.Magic == 267)
					{
						if (nTHeader.OptionalHeader.DataDirectory[14].Size > 0u)
						{
							text = "Image contains a CLR runtime header. Currently only native binaries are supported; no .NET dependent libraries.";
						}
					}
					else
					{
						text = "File is of the PE32+ format. Currently support only extends to PE32 images. Either recompile the binary as x86, or choose a different target.";
					}
				}
				else
				{
					text = "Invalid NT header found in image.";
				}
			}
			else
			{
				text = "Invalid DOS Header found in image";
			}
			if (string.IsNullOrEmpty(text))
			{
				this.NTHeader = nTHeader;
				this.DOSHeader = dOSHeader;
				return;
			}
			base.Dispose();
			throw new ArgumentException(text);
		}

		public IEnumerable<IMAGE_SECTION_HEADER> EnumSectionHeaders()
		{
			uint numberOfSections = (uint)this.NTHeader.FileHeader.NumberOfSections;
			long num = (long)((ulong)((uint)this.NTHeader.FileHeader.SizeOfOptionalHeader + typeof(IMAGE_FILE_HEADER).SizeOf() + 4u + this.DOSHeader.e_lfanew));
			uint num2 = typeof(IMAGE_SECTION_HEADER).SizeOf();
			for (uint num3 = 0u; num3 < numberOfSections; num3 += 1u)
			{
				IMAGE_SECTION_HEADER iMAGE_SECTION_HEADER;
				if (base.Read<IMAGE_SECTION_HEADER>(num + (long)((ulong)(num3 * num2)), SeekOrigin.Begin, out iMAGE_SECTION_HEADER))
				{
					yield return iMAGE_SECTION_HEADER;
				}
			}
			yield break;
		}

		public IEnumerable<IMAGE_IMPORT_DESCRIPTOR> EnumImports()
		{
			IMAGE_DATA_DIRECTORY iMAGE_DATA_DIRECTORY = this.NTHeader.OptionalHeader.DataDirectory[1];
			if (iMAGE_DATA_DIRECTORY.Size > 0u)
			{
				uint num = this.GetPtrFromRVA(iMAGE_DATA_DIRECTORY.VirtualAddress);
				uint num2 = typeof(IMAGE_IMPORT_DESCRIPTOR).SizeOf();
				IMAGE_IMPORT_DESCRIPTOR iMAGE_IMPORT_DESCRIPTOR;
				while (base.Read<IMAGE_IMPORT_DESCRIPTOR>((long)((ulong)num), SeekOrigin.Begin, out iMAGE_IMPORT_DESCRIPTOR) && iMAGE_IMPORT_DESCRIPTOR.OriginalFirstThunk > 0u && iMAGE_IMPORT_DESCRIPTOR.Name > 0u)
				{
					yield return iMAGE_IMPORT_DESCRIPTOR;
					num += num2;
				}
			}
			yield break;
		}

		public byte[] ToArray()
		{
			return base.GetUnderlyingData();
		}

		private IMAGE_SECTION_HEADER GetEnclosingSectionHeader(uint rva)
		{
			foreach (IMAGE_SECTION_HEADER current in this.EnumSectionHeaders())
			{
				if (rva >= current.VirtualAddress && rva < current.VirtualAddress + ((current.VirtualSize > 0u) ? current.VirtualSize : current.SizeOfRawData))
				{
					return current;
				}
			}
			throw new EntryPointNotFoundException("RVA does not exist within any of the current sections.");
		}

		public uint GetPtrFromRVA(uint rva)
		{
			IMAGE_SECTION_HEADER enclosingSectionHeader = this.GetEnclosingSectionHeader(rva);
			return rva - (enclosingSectionHeader.VirtualAddress - enclosingSectionHeader.PointerToRawData);
		}
	}
}
