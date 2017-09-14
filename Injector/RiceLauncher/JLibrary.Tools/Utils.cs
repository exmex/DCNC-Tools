using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace JLibrary.Tools
{
	public static class Utils
	{
		public static string WriteTempData(byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			string text = null;
			try
			{
				text = Path.GetTempFileName();
			}
			catch (IOException)
			{
				text = Path.Combine(Directory.GetCurrentDirectory(), Path.GetRandomFileName());
			}
			try
			{
				File.WriteAllBytes(text, data);
			}
			catch
			{
				text = null;
			}
			return text;
		}

		public static T DeepClone<T>(T obj)
		{
			T result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(memoryStream, obj);
				memoryStream.Position = 0L;
				result = (T)((object)binaryFormatter.Deserialize(memoryStream));
			}
			return result;
		}

		public static uint SizeOf(this Type t)
		{
			return (uint)Marshal.SizeOf(t);
		}
	}
}
