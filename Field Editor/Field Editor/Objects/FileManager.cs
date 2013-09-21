using System;
using System.IO;

namespace FieldEditor
{
	public static class FileManager
	{
		/// <summary>
		/// Reads a binary field from data file.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="kind"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public static object BinRead(FileStream stream, Kind kind, long offset)
		{
			var reader = new BinaryReader(stream);
			stream.Position = offset;
			switch (kind)
			{
				case Kind.Byte:
					return reader.ReadByte();
				case Kind.Int16:
					return reader.ReadInt16();
				case Kind.Int32:
					return reader.ReadInt32();
				case Kind.Int64:
					return reader.ReadInt64();
				case Kind.Single:
					return reader.ReadSingle();
				case Kind.Double:
					return reader.ReadDouble();
				default:
					throw new Exception("Invalid type code.");
			}
		}
		/// <summary>
		/// Writes a binary field to data file.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="kind"></param>
		/// <param name="offset"></param>
		/// <param name="v"></param>
		public static void BinWrite(FileStream stream, Kind kind, long offset, object v)
		{
			var v_str = v.ToString();
			stream.Position = offset;
			var writer = new BinaryWriter(stream);
			switch (kind)
			{
				case Kind.Byte:
					writer.Write(byte.Parse(v_str));
					break;
				case Kind.Int16:
					writer.Write(short.Parse(v_str));
					break;
				case Kind.Int32:
					writer.Write(int.Parse(v_str));
					break;
				case Kind.Int64:
					writer.Write(long.Parse(v_str));
					break;
				case Kind.Single:
					writer.Write(float.Parse(v_str));
					break;
				case Kind.Double:
					writer.Write(double.Parse(v_str));
					break;
				default:
					throw new Exception("Invalid type code.");
			}
			writer.Flush();
		}
	}
}