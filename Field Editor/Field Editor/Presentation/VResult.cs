using System.Windows.Controls;
using FieldEditor.CustomAspects;

namespace FieldEditor
{
	public static class VResult
	{
		public static ValidationResult Valid { get { return ValidationResult.ValidResult; } }

		public static ValidationResult Invalid(object o)
		{
			return new ValidationResult(false, o);
		}
	}
}