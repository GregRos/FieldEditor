using System;

namespace FieldEditor
{
	/// <summary>
	/// A validity state. Anything besides None means something is wrong with the field.
	/// </summary>
	[Flags]
	public enum Invalid
	{
		None = 0,
		/// <summary>
		/// Invalid name (e.g. doesn't exist, white space).
		/// </summary>
		Name = 0x1,
		/// <summary>
		/// Invalid group (e.g. doesn't exist, white space).
		/// </summary>
		Group = 0x4,
		/// <summary>
		/// Invalid path (e.g. doesn't exist)
		/// </summary>
		Path = 0x8,
		/// <summary>
		/// Invalid offset (e.g. negative, Kind.Width + Offset exceeds length of file)
		/// </summary>
		Offset = 0x10,
		/// <summary>
		/// Invalid value (e.g. wrong data type, is null).
		/// </summary>
		Value = 0x40,
	}
}