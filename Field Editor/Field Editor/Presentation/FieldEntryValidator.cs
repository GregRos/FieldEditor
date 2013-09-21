using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace FieldEditor
{
	public class FieldEntryValidator : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			var row = (value as BindingGroup).Owner as DataGridRow;
			var newField = (value as BindingGroup).Items[0] as Field;
			var state = newField.CheckValidity;
			if (state.HasFlag(Invalid.Name)) return VResult.Invalid("Valid name is required.");
			if (state.HasFlag(Invalid.Group)) return VResult.Invalid("Valid group is required.");
			if (state.HasFlag(Invalid.Path)) return VResult.Invalid("FileName is invalid.");
			if (state.HasFlag(Invalid.Offset)) return VResult.Invalid("Offset is invalid.");
			if (state.HasFlag(Invalid.Value)) return VResult.Invalid("Value is invalid.");
			return VResult.Valid;
		}
	}
}