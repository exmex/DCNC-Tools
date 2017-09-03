using System;
using System.Runtime.InteropServices;

namespace JLibrary.Tools
{
	[Serializable]
	public class UnmanagedBuffer : ErrorBase, IDisposable
	{
		private bool _disposed;

		public IntPtr Pointer
		{
			get;
			private set;
		}

		public int Size
		{
			get;
			private set;
		}

		public UnmanagedBuffer(int cbneeded)
		{
			if (cbneeded > 0)
			{
				this.Pointer = Marshal.AllocHGlobal(cbneeded);
				this.Size = cbneeded;
				return;
			}
			this.Pointer = IntPtr.Zero;
			this.Size = 0;
		}

		public bool Commit(byte[] data, int index, int count)
		{
			if (data != null && this.Alloc(count))
			{
				Marshal.Copy(data, index, this.Pointer, count);
				return true;
			}
			if (data == null)
			{
				this.SetLastError(new ArgumentException("Attempting to commit a null reference", "data"));
			}
			return false;
		}

		public bool Commit<T>(T data) where T : struct
		{
			bool result;
			try
			{
				if (this.Alloc(Marshal.SizeOf(typeof(T))))
				{
					Marshal.StructureToPtr(data, this.Pointer, false);
					result = true;
				}
				else
				{
					result = false;
				}
			}
			catch (Exception lastError)
			{
				result = this.SetLastError(lastError);
			}
			return result;
		}

		public bool SafeDecommit<T>() where T : struct
		{
			bool result;
			try
			{
				if (this.Size < Marshal.SizeOf(typeof(T)))
				{
					throw new InvalidCastException("Not enough unmanaged memory is allocated to contain this structure type.");
				}
				Marshal.DestroyStructure(this.Pointer, typeof(T));
				result = true;
			}
			catch (Exception lastError)
			{
				result = this.SetLastError(lastError);
			}
			return result;
		}

		public bool Read<TResult>(out TResult data) where TResult : struct
		{
			data = default(TResult);
			bool result;
			try
			{
				if (this.Size < Marshal.SizeOf(typeof(TResult)))
				{
					throw new InvalidCastException("Not enough unmanaged memory is allocated to contain this structure type.");
				}
				data = (TResult)((object)Marshal.PtrToStructure(this.Pointer, typeof(TResult)));
				result = true;
			}
			catch (Exception lastError)
			{
				result = this.SetLastError(lastError);
			}
			return result;
		}

		public byte[] Read(int count)
		{
			byte[] result;
			try
			{
				if (count > this.Size || count <= 0)
				{
					throw new ArgumentException("There is either not enough memory allocated to read 'count' bytes, or 'count' is negative (" + count.ToString() + ")", "count");
				}
				byte[] array = new byte[count];
				Marshal.Copy(this.Pointer, array, 0, count);
				result = array;
			}
			catch (Exception lastError)
			{
				this.SetLastError(lastError);
				result = null;
			}
			return result;
		}

		public bool Translate<TSource>(TSource data, out byte[] buffer) where TSource : struct
		{
			buffer = null;
			if (this.Commit<TSource>(data))
			{
				buffer = this.Read(Marshal.SizeOf(typeof(TSource)));
				this.SafeDecommit<TSource>();
			}
			return buffer != null;
		}

		public bool Translate<TResult>(byte[] buffer, out TResult result) where TResult : struct
		{
			result = default(TResult);
			if (buffer == null)
			{
				return this.SetLastError(new ArgumentException("Attempted to translate a null reference to a structure.", "buffer"));
			}
			return this.Commit(buffer, 0, buffer.Length) && this.Read<TResult>(out result);
		}

		public bool Translate<TSource, TResult>(TSource data, out TResult result) where TSource : struct where TResult : struct
		{
			result = default(TResult);
			return this.Commit<TSource>(data) && this.Read<TResult>(out result) && this.SafeDecommit<TSource>();
		}

		public bool Resize(int size)
		{
			if (size < 0)
			{
				return this.SetLastError(new ArgumentException("Attempting to resize to less than zero bytes of memory", "size"));
			}
			if (size == this.Size)
			{
				return true;
			}
			if (size > this.Size)
			{
				return this.Alloc(size);
			}
			bool result;
			try
			{
				if (size == 0)
				{
					Marshal.FreeHGlobal(this.Pointer);
					this.Pointer = IntPtr.Zero;
				}
				else if (size > 0)
				{
					this.Pointer = Marshal.ReAllocHGlobal(this.Pointer, new IntPtr(size));
				}
				this.Size = size;
				result = true;
			}
			catch (Exception lastError)
			{
				result = this.SetLastError(lastError);
			}
			return result;
		}

		private bool Alloc(int cb)
		{
			bool result;
			try
			{
				if (cb > this.Size)
				{
					this.Pointer = ((this.Pointer == IntPtr.Zero) ? Marshal.AllocHGlobal(cb) : Marshal.ReAllocHGlobal(this.Pointer, new IntPtr(cb)));
					this.Size = cb;
				}
				result = true;
			}
			catch (Exception lastError)
			{
				result = this.SetLastError(lastError);
			}
			return result;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this._disposed)
			{
				if (disposing)
				{
					this.Resize(0);
				}
				this._disposed = true;
			}
		}
	}
}
