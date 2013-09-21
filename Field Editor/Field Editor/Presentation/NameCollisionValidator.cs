using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FieldEditor
{
	public class NameCollisionValidator : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			var bindingGrp = (value as BindingGroup);
			var row = bindingGrp.Owner as DataGridRow;
			var newField = bindingGrp.Items[0] as Field;
			var window = Window.GetWindow(row) as MainWindow;
			var fields = window.Fields;
			var occurrences = fields.CountOccurrences(newField, Field.NameEqualityComparer);
			return occurrences == 1 ? VResult.Valid : VResult.Invalid("This name-subgroup-group combination already exists.");
		}
	}
}