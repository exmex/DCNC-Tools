using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace JLibrary.Tools
{
	[Serializable]
	public class MemoryIterator : ErrorBase, IDisposable
	{
		private MemoryStream _base;

		private UnmanagedBuffer _ubuffer;

		private bool _disposed;

		public MemoryIterator(byte[] iterable)
		{
			if (iterable == null)
			{
				throw new ArgumentException("Unable to iterate a null reference", "iterable");
			}
			this._base = new MemoryStream(iterable, 0, iterable.Length, true);
			this._ubuffer = new UnmanagedBuffer(256);
		}

		protected byte[] GetUnderlyingData()
		{
			return this._base.ToArray();
		}

		public bool Read<TResult>(out TResult result) where TResult : struct
		{
			return this.Read<TResult>(0L, SeekOrigin.Current, out result);
		}

		public long Seek(long offset, SeekOrigin origin)
		{
			return this._base.Seek(offset, origin);
		}

		public bool Read<TResult>(long offset, SeekOrigin origin, out TResult result) where TResult : struct
		{
			result = default(TResult);
			bool result2;
			try
			{
				this._base.Seek(offset, origin);
				byte[] array = new byte[Marshal.SizeOf(typeof(TResult))];
				this._base.Read(array, 0, array.Length);
				if (!this._ubuffer.Translate<TResult>(array, out result))
				{
					throw this._ubuffer.GetLastError();
				}
				result2 = true;
			}
			catch (Exception lastError)
			{
				result2 = base.SetLastError(lastError);
			}
			return result2;
		}

		public bool ReadString(long offset, SeekOrigin origin, out string lpBuffer, int len = -1, Encoding stringEncoding = null)
		{
			lpBuffer = null;
			byte[] array = new byte[(len > 0) ? len : 64];
			if (stringEncoding == null)
			{
				stringEncoding = Encoding.ASCII;
			}
			bool result;
			try
			{
				this._base.Seek(offset, origin);
				StringBuilder stringBuilder = new StringBuilder((len > 0) ? len : 260);
				int num = -1;
				int num2 = 0;
				int num3;
				while (num == -1 && (num3 = this._base.Read(array, 0, array.Length)) > 0)
				{
					stringBuilder.Append(stringEncoding.GetString(array));
					num = stringBuilder.ToString().IndexOf('\0', num2);
					num2 += num3;
					if (len > 0 && num2 >= len)
					{
						break;
					}
				}
				if (num > -1)
				{
					lpBuffer = stringBuilder.ToString().Substring(0, num);
				}
				else if (num2 >= len && len > 0)
				{
					lpBuffer = stringBuilder.ToString().Substring(0, len);
				}
				result = (lpBuffer != null);
			}
			catch (Exception lastError)
			{
				result = this.SetLastError(lastError);
			}
			return result;
		}

		public bool Read(long offset, SeekOrigin origin, byte[] buffer)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer", "Parameter cannot be null");
			}
			try
			{
				this._base.Seek(offset, origin);
				this._base.Read(buffer, 0, buffer.Length);
			}
			catch (Exception lastError)
			{
				this.SetLastError(lastError);
				buffer = null;
			}
			return buffer != null;
		}

		public bool Write<TSource>(long offset, SeekOrigin origin, TSource data) where TSource : struct
		{
			bool result;
			try
			{
				this._base.Seek(offset, origin);
				byte[] array = null;
				if (!this._ubuffer.Translate<TSource>(data, out array))
				{
					throw this._ubuffer.GetLastError();
				}
				this._base.Write(array, 0, array.Length);
				result = true;
			}
			catch (Exception lastError)
			{
				result = this.SetLastError(lastError);
			}
			return result;
		}

		public bool Write(long offset, SeekOrigin origin, byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("Parameter 'data' cannot be null");
			}
			bool result;
			try
			{
				this._base.Seek(offset, origin);
				this._base.Write(data, 0, data.Length);
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

		protected virtual void Dispose(bool disposing)
		{
			if (!this._disposed)
			{
				if (disposing)
				{
					this._ubuffer.Dispose();
					this._base.Dispose();
				}
				this._disposed = true;
			}
		}
	}
}
