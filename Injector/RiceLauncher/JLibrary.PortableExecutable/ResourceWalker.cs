using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JLibrary.PortableExecutable
{
	public class ResourceWalker
	{
		public abstract class ResourceObject
		{
			private string _name;

			protected uint _root;

			protected PortableExecutable _owner;

			protected IMAGE_RESOURCE_DIRECTORY_ENTRY _entry;

			public string Name
			{
				get
				{
					return this._name;
				}
			}

			public int Id
			{
				get
				{
					if (!this.IsNamedResource)
					{
						return (int)this._entry.IntegerId;
					}
					return -1;
				}
			}

			public bool IsNamedResource
			{
				get;
				protected set;
			}

			public ResourceObject(PortableExecutable owner, IMAGE_RESOURCE_DIRECTORY_ENTRY entry, bool named, uint root)
			{
				this._owner = owner;
				this._entry = entry;
				this.IsNamedResource = named;
				if (named)
				{
					ushort num = 0;
					if (owner.Read<ushort>((long)((ulong)(root + (entry.NameRva & 2147483647u))), SeekOrigin.Begin, out num))
					{
						byte[] array = new byte[(int)num << 1];
						if (owner.Read(0L, SeekOrigin.Current, array))
						{
							this._name = Encoding.Unicode.GetString(array);
						}
					}
					if (this._name == null)
					{
						throw owner.GetLastError();
					}
				}
				this._root = root;
			}
		}

		public class ResourceFile : ResourceWalker.ResourceObject
		{
			private IMAGE_RESOURCE_DATA_ENTRY _base;

			public ResourceFile(PortableExecutable owner, IMAGE_RESOURCE_DIRECTORY_ENTRY entry, bool named, uint root) : base(owner, entry, named, root)
			{
				if (!owner.Read<IMAGE_RESOURCE_DATA_ENTRY>((long)((ulong)(this._root + entry.DataEntryRva)), SeekOrigin.Begin, out this._base))
				{
					throw owner.GetLastError();
				}
			}

			public byte[] GetData()
			{
				byte[] array = new byte[this._base.Size];
				if (!this._owner.Read((long)((ulong)this._owner.GetPtrFromRVA(this._base.OffsetToData)), SeekOrigin.Begin, array))
				{
					throw this._owner.GetLastError();
				}
				return array;
			}
		}

		public class ResourceDirectory : ResourceWalker.ResourceObject
		{
			private const uint SZ_ENTRY = 8u;

			private const uint SZ_DIRECTORY = 16u;

			private IMAGE_RESOURCE_DIRECTORY _base;

			private ResourceWalker.ResourceFile[] _files;

			private ResourceWalker.ResourceDirectory[] _dirs;

			public ResourceWalker.ResourceFile[] Files
			{
				get
				{
					if (this._files == null)
					{
						this.Initialize();
					}
					return this._files;
				}
			}

			public ResourceWalker.ResourceDirectory[] Directories
			{
				get
				{
					if (this._dirs == null)
					{
						this.Initialize();
					}
					return this._dirs;
				}
			}

			private void Initialize()
			{
				List<ResourceWalker.ResourceDirectory> list = new List<ResourceWalker.ResourceDirectory>();
				List<ResourceWalker.ResourceFile> list2 = new List<ResourceWalker.ResourceFile>();
				int numberOfNamedEntries = (int)this._base.NumberOfNamedEntries;
				for (int i = 0; i < numberOfNamedEntries + (int)this._base.NumberOfIdEntries; i++)
				{
					IMAGE_RESOURCE_DIRECTORY_ENTRY entry;
					if (this._owner.Read<IMAGE_RESOURCE_DIRECTORY_ENTRY>((long)((ulong)(this._root + 16u + (this._entry.SubdirectoryRva ^ 2147483648u)) + (ulong)((long)i * 8L)), SeekOrigin.Begin, out entry))
					{
						if ((entry.SubdirectoryRva & 2147483648u) != 0u)
						{
							list.Add(new ResourceWalker.ResourceDirectory(this._owner, entry, i < numberOfNamedEntries, this._root));
						}
						else
						{
							list2.Add(new ResourceWalker.ResourceFile(this._owner, entry, i < numberOfNamedEntries, this._root));
						}
					}
				}
				this._files = list2.ToArray();
				this._dirs = list.ToArray();
			}

			public ResourceDirectory(PortableExecutable owner, IMAGE_RESOURCE_DIRECTORY_ENTRY entry, bool named, uint root) : base(owner, entry, named, root)
			{
				if (!owner.Read<IMAGE_RESOURCE_DIRECTORY>((long)((ulong)(root + (entry.SubdirectoryRva ^ 2147483648u))), SeekOrigin.Begin, out this._base))
				{
					throw owner.GetLastError();
				}
			}
		}

		public ResourceWalker.ResourceDirectory Root
		{
			get;
			private set;
		}

		public ResourceWalker(PortableExecutable image)
		{
			IMAGE_DATA_DIRECTORY iMAGE_DATA_DIRECTORY = image.NTHeader.OptionalHeader.DataDirectory[2];
			if (iMAGE_DATA_DIRECTORY.VirtualAddress <= 0u || iMAGE_DATA_DIRECTORY.Size <= 0u)
			{
				return;
			}
			uint ptrFromRVA;
			IMAGE_RESOURCE_DIRECTORY iMAGE_RESOURCE_DIRECTORY;
			if (image.Read<IMAGE_RESOURCE_DIRECTORY>((long)((ulong)(ptrFromRVA = image.GetPtrFromRVA(iMAGE_DATA_DIRECTORY.VirtualAddress))), SeekOrigin.Begin, out iMAGE_RESOURCE_DIRECTORY))
			{
				this.Root = new ResourceWalker.ResourceDirectory(image, new IMAGE_RESOURCE_DIRECTORY_ENTRY
				{
					SubdirectoryRva = 2147483648u
				}, false, ptrFromRVA);
				return;
			}
			throw image.GetLastError();
		}
	}
}
