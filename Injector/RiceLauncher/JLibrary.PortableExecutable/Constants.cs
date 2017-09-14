using System;

namespace JLibrary.PortableExecutable
{
	public static class Constants
	{
		public const ushort DOS_SIGNATURE = 23117;

		public const uint NT_SIGNATURE = 17744u;

		public const ushort PE32_FORMAT = 267;

		public const ushort PE32P_FORMAT = 523;

		public const uint RT_MANIFEST = 24u;

		public const uint CREATEPROCESS_MANIFEST_RESOURCE_ID = 1u;

		public const uint ISOLATIONAWARE_MANIFEST_RESOURCE_ID = 2u;

		public const uint ISOLATIONAWARE_NOSTATICIMPORT_MANIFEST_RESOURCE_ID = 3u;
	}
}
