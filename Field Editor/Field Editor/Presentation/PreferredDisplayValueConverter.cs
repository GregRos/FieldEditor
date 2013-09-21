using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;

namespace FieldEditor
{
	public class PreferredDisplayValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var field = parameter as Field;
			if (targetType == typeof (string))
			{
				if (field == null) return null;
				var str = field.Value == null ? "" : field.Value.ToString();
				if (field.CheckValidity.HasFlag(Invalid.Value))
					return str;
				switch (field.Kind)
				{
					case Kind.Double:
					case Kind.Single:
						var a = double.Parse(str);
						return a % 1 == 0 ? "{0:0.0}".FormatWith(a) : a.ToString();
					default:
						return str;
				}
			}
			throw new ArgumentException("Invalid conversion parameters.");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}