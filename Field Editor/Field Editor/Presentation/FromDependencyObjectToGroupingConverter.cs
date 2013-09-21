using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FieldEditor
{
	internal class FromDependencyObjectToGroupingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var target = value as DependencyObject;
			if (target == null) return null;
			var groupings = target.GetVisualParents<GroupItem>().ToArray();
			var sgView = groupings[0].DataContext as CollectionViewGroup;
			var sgName = sgView.Name as string;
			var gView = groupings[1].DataContext as CollectionViewGroup;
			var gName = gView.Name as string;
			return new Field.GroupingIdentifier
			       {
				       Group = gName,
				       Subgroup = sgName
			       };
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}