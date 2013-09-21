using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using FieldEditor.CustomAspects;

namespace FieldEditor
{
	public class Settings : INotifyPropertyChanged
	{
		private static readonly XmlSerializer _serializer = new XmlSerializer(typeof (Settings));

		public static readonly Settings DefaultSettings =
			new Settings
			{
				Path_MostRecentFile = null
			};

		public event PropertyChangedEventHandler PropertyChanged;

		public static Settings DeserializeOrDefault(string path)
		{
			if (!File.Exists(path))
				return DefaultSettings;
			using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				file.Position = 0;
				if (!_serializer.CanDeserialize(new XmlTextReader(file)))
					return DefaultSettings;
				file.Position = 0;
				return _serializer.Deserialize(file) as Settings;
			}
		}

		public string Path_DataFileFolder { get; set; }

		public string Path_MostRecentFile { get; set; }

		public void SerializeTo(string path)
		{
			using (var file = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
			{
				file.SetLength(0);
				file.Flush();
				_serializer.Serialize(file, this);
				file.Flush();
			}
		}
	}
}