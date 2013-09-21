using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using FieldEditor.CustomAspects;

namespace FieldEditor
{
	/// <summary>
	/// Encapsulates a binary field within a file.
	/// </summary>
	public class Field : INotifyPropertyChanged, IEditableObject
	{
		/// <summary>
		/// Specifies whether the action is write to file or read to file.
		/// </summary>
		public enum FileAction
		{
			Read,
			Write
		}
		/// <summary>
		/// A factory that constructs instances of the Field class associated with a given data folder.
		/// </summary>
		public class Factory
		{
			private static readonly XmlSerializer serializer = new XmlSerializer(typeof (Xml.Container));
			private readonly string _relativePath;

			public Factory(string relativePath)
			{
				_relativePath = relativePath;
			}

			public string RelativePath { get { return _relativePath; } }

			public Field Clone(Field o)
			{
				return new Field(this)
				       {
					       Group = o.Group,
					       Kind = o.Kind,
					       FileName = o.FileName,
					       Name = o.Name,
					       Notes = o.Notes,
					       Subgroup = o.Subgroup,
					       DefaultValue = o.DefaultValue,
					       Offset = o.Offset,
					       Value = o.Value,
					       IsSaved = o.IsSaved
				       };
			}

			public Field Create()
			{
				return new Field(this);
			}

			public Field FromBasic(Xml b)
			{
				return new Field(this)
				       {
					       Name = b.Name,
					       FileName = Path.GetFileName(b.Path),
					       Group = b.Group,
					       Subgroup = b.Subgroup,
					       DefaultValue = b.DefaultValue,
					       Notes = b.Notes,
					       Offset = b.Offset,
					       Kind = b.Kind,
				       };
			}

			public bool IsValidFieldFile(string path)
			{
				if (!File.Exists(path)) return false;
				using (var file = File.Open(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
				{
					return serializer.CanDeserialize(XmlReader.Create(file));
				}
			}

			public IEnumerable<Field> LoadFieldsFrom(string path)
			{
				if (!File.Exists(path)) return Enumerable.Empty<Field>();
				using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					var output = serializer.Deserialize(file) as Xml.Container;
					return output.Fields.Select(FromBasic).ToArray();
				}
			}

			public void SaveFieldsTo(string path, IEnumerable<Field> fields)
			{
				using (var file = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
				{
					file.SetLength(0);
					file.Flush();
					var oFields = new Xml.Container {Fields = fields.Select(x => x.AsBasic()).ToArray()};
					serializer.Serialize(file, oFields);
					file.Flush();
				}
			}
		}

		public struct GroupingIdentifier
		{
			public string Group;
			public string Subgroup;

			public bool IsMatch(Field fld)
			{
				return fld.Group == Group && fld.Subgroup == Subgroup;
			}
		}
		/// <summary>
		/// A POCO xml container for serializing and deserializing objects of type Field.
		/// </summary>
		public class Xml
		{
			[XmlInclude(typeof (Xml))]
			[XmlRoot("SavedFields")]
			public class Container
			{
				[XmlArray("Fields")]
				[XmlArrayItem("Field")]
				public Xml[] Fields { get; set; }
			}

			public object DefaultValue;
			public string Group;
			public Kind Kind;
			public string Name;
			public string Notes;
			public long Offset;
			public string Path;
			public string Subgroup;
		}

		private readonly Factory _creator;
		private Field _snapshot;
		private object _value;
		public event PropertyChangedEventHandler PropertyChanged = Handle_ReloadValue;

		/// <summary>
		///   NEVER USE THIS CONSTRUCTOR. This is a hack to allow datagrid compatibility, and it never gets called. Invoking it throws an exception.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This constructor should never be called. This is a hack to allow DataGrid compatibility.", true)]
		public Field()
		{
			throw new InvalidOperationException("This constructor should never be called. This is a hack to allow DataGrid compatibility.");
		}

		private Field(Factory creator)
		{
			_creator = creator;
		}

		private class NameEqualityComparerClass : IEqualityComparer<Field>
		{
			public bool Equals(Field x, Field y)
			{
				return x.Subgroup == y.Subgroup && x.Group == y.Group && x.Name == y.Name;
			}

			public int GetHashCode(Field obj)
			{
				return obj.Group.GetHashCode() ^ obj.Subgroup.GetHashCode() ^ obj.Name.GetHashCode();
			}
		}
		/// <summary>
		/// Returns an equality comparer used to check for group-subgroup-name collisions.
		/// </summary>
		public static IEqualityComparer<Field> NameEqualityComparer { get { return new NameEqualityComparerClass(); } }
		/// <summary>
		/// Performs an operation to read or write multiple fields to a field file.
		/// </summary>
		/// <param name="flds"></param>
		/// <param name="op"></param>
		public static void BulkFileOperation(IEnumerable<Field> flds, FileAction op)
		{
			var loadedFiles = new Dictionary<string, FileStream>();
			var accessType = op == FileAction.Read ? FileAccess.Read : FileAccess.Write;
			flds = from fld in flds
			       let validity = fld.CheckValidity
			       where !validity.HasAnyFlag(Invalid.Path, Invalid.Offset)
			       where op == FileAction.Read || !validity.HasFlag(Invalid.Value)
			       select fld;
			try
			{
				foreach (var fld in flds.Distinct(x => x.FileName))
				{
					var stream = File.Open(fld.FilePath, FileMode.Open, accessType, FileShare.ReadWrite);
					loadedFiles.Add(fld.FileName, stream);
				}
				foreach (var fileGroup in from fld in flds
				                          group fld by fld.FileName)
				{
					var stream = loadedFiles[fileGroup.Key];
					foreach (var field in fileGroup)
					{
						if (op == FileAction.Read)
							field.Value = FileManager.BinRead(stream, field.Kind, field.Offset);
						else
							FileManager.BinWrite(stream, field.Kind, field.Offset, field.Value);
						field.IsSaved = true;
					}
				}
			}
			catch (IOException ex)
			{
				var err = string.Format(
					@"An error has occurred while opening a file for reading/writing. Possible causes for the error include:
1. File doesn't exist
2. File is read-only
3. Lack of permissions
4. File is being used by another process.
More information: 
{0}
", ex.Message);

				MessageBox.Show(err, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			finally
			{
				foreach (var file in loadedFiles.Values)
				{
					file.Close();
				}
			}
		}

		private static void Handle_ReloadValue(object sender, PropertyChangedEventArgs e)
		{
			var fld = sender as Field;
			if (new[] {"FileName", "Offset", "Kind"}.Contains(e.PropertyName))
				fld.ReloadValue();
		}
		/// <summary>
		/// Returns a validity state by performing checks
		/// </summary>
		public Invalid CheckValidity
		{
			get
			{
				var state = Invalid.None;
				if (Name.IsNullOrWhiteSpace()) state |= Invalid.Name;
				if (Group.IsNullOrWhiteSpace()) state |= Invalid.Group;

				if (!File.Exists(FilePath)) state |= Invalid.Path;
				if (Offset < 0) state |= Invalid.Offset;
				if (!state.HasAnyFlag(Invalid.Path, Invalid.Offset))
				{
					var length = new FileInfo(FilePath).Length;
					if (Offset + Kind.Width() >= length)
						state |= Invalid.Offset;
				}
				if (!Kind.IsLegalValue(Value))
					state |= Invalid.Value;
				return state;
			}
		}

		public Factory Creator { get { return _creator; } }

		[NotifyPropertyChangedAspect]
		public object DefaultValue { get; set; }

		[NotifyPropertyChangedAspect("Value")]
		public string DisplayValue { get { return Kind.Format(Value); } }

		[NotifyPropertyChangedAspect]
		public string FileName { get; set; }

		[NotifyPropertyChangedAspect("FileName")]
		public string FilePath { get { return FileName == null ? null : Path.Combine(Creator.RelativePath, FileName); } }

		[NotifyPropertyChangedAspect]
		public string Group { get; set; }

		[NotifyPropertyChangedAspect]
		public bool InEditMode { get; private set; }

		[NotifyPropertyChangedAspect]
		public bool IsSaved { get; private set; }

		[NotifyPropertyChangedAspect]
		public Kind Kind { get; set; }

		[NotifyPropertyChangedAspect]
		public string Name { get; set; }

		[NotifyPropertyChangedAspect]
		public string Notes { get; set; }

		[NotifyPropertyChangedAspect]
		public long Offset { get; set; }

		[NotifyPropertyChangedAspect]
		public string Subgroup { get; set; }

		[NotifyPropertyChangedAspect]
		public object Value
		{
			get { return _value; }
			set
			{
				_value = value;
				IsSaved = false;
			}
		}

		public Xml AsBasic()
		{
			var f = this;
			return new Xml
			       {
				       DefaultValue = f.DefaultValue,
				       Group = f.Group,
				       Subgroup = f.Subgroup,
				       Kind = f.Kind,
				       Notes = f.Notes,
				       Name = f.Name,
				       Offset = f.Offset,
				       Path = f.FileName
			       };
		}

		public void BeginEdit()
		{
			_snapshot = Clone();
			InEditMode = true;
		}

		public void CancelEdit()
		{
			if (InEditMode)
			{
				RevertFrom(_snapshot);
				InEditMode = false;
			}
		}

		public Field Clone()
		{
			return Creator.Clone(this);
		}

		public void EndEdit()
		{
			InEditMode = false;
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ReloadValue()
		{
			BulkFileOperation(new[] {this}, FileAction.Read);
		}

		private void RevertFrom(Field other)
		{
			Group = other.Group;
			Kind = other.Kind;
			Name = other.Name;
			Offset = other.Offset;
			FileName = other.FileName;
			Notes = other.Notes;
			Subgroup = other.Subgroup;
			DefaultValue = other.DefaultValue;
			Value = other.Value;
			IsSaved = other.IsSaved;
		}
	}
}