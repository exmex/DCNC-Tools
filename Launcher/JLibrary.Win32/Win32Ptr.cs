using System;

namespace JLibrary.Win32
{
	public static class Win32Ptr
	{
		public static IntPtr Create(long value)
		{
			return new IntPtr((int)value);
		}

		public static IntPtr Add(this IntPtr ptr, long val)
		{
			return new IntPtr((int)((long)ptr.ToInt32() + val));
		}

		public static IntPtr Add(this IntPtr ptr, IntPtr val)
		{
			return new IntPtr(ptr.ToInt32() + val.ToInt32());
		}

		public static IntPtr Subtract(this IntPtr ptr, long val)
		{
			return new IntPtr((int)(ptr.ToInt64() - val));
		}

		public static IntPtr Subtract(this IntPtr ptr, IntPtr val)
		{
			return new IntPtr((int)(ptr.ToInt64() - val.ToInt64()));
		}

		public static bool IsNull(this IntPtr ptr)
		{
			return ptr == IntPtr.Zero;
		}

		public static bool IsNull(this UIntPtr ptr)
		{
			return ptr == UIntPtr.Zero;
		}

		public static bool Compare(this IntPtr ptr, long value)
		{
			return ptr.ToInt64() == value;
		}
	}
}
