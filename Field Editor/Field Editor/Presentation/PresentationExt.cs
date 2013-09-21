using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace FieldEditor
{
	internal static class PresentationExt
	{
		public static IEnumerable<T> GetLogicalParents<T>(this DependencyObject current) where T : DependencyObject
		{
			if (current == null) return null;

			var parents = new Collection<T>();
			for (var cur = current; cur != null; cur = LogicalTreeHelper.GetParent(cur))
			{
				var typed = cur as T;
				if (typed != null) parents.Add(typed);
			}
			return parents;
		}

		public static IEnumerable<T> GetVisualChildren<T>(this DependencyObject current) where T : DependencyObject
		{
			if (current == null)
				return null;

			var children = new Collection<T>();

			GetVisualChildren(current, children);

			return children;
		}

		private static void GetVisualChildren<T>(DependencyObject current, Collection<T> children) where T : DependencyObject
		{
			if (current != null)
			{
				var typed = current as T;
				if (typed != null) children.Add(typed);

				for (var i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
				{
					GetVisualChildren(VisualTreeHelper.GetChild(current, i), children);
				}
			}
		}

		public static IEnumerable<T> GetVisualParents<T>(this DependencyObject current) where T : DependencyObject
		{
			if (current == null) return null;

			var parents = new Collection<T>();
			for (var cur = current; cur != null; cur = VisualTreeHelper.GetParent(cur))
			{
				var typed = cur as T;
				if (typed != null) parents.Add(typed);
			}
			return parents;
		}
	}
}