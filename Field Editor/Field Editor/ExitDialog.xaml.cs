using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using FieldEditor.CustomAspects;

namespace FieldEditor
{
	/// <summary>
	///   Interaction logic for ExitDialog.xaml
	/// </summary>
	public partial class ExitDialog : Window, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public ExitDialog(string prompt)
		{
			Prompt = prompt;
			InitializeComponent();
		}

		[NotifyPropertyChangedAspect]
		public string Prompt { get; set; }

		public ExitDialogResponse Response { get; private set; }

		private void ClickedButton(object sender, RoutedEventArgs e)
		{
			var btn = e.Source as Button;
			DialogResult = true;
			switch (btn.Name)
			{
				case "btnCommit":
					Response = ExitDialogResponse.Commit;
					break;
				case "btnBoth":
					Response = ExitDialogResponse.Both;
					break;
				case "btnSave":
					Response = ExitDialogResponse.Save;
					break;
				case "btnExit":
					Response = ExitDialogResponse.Exit;
					break;
				case "btnCancel":
					Response = ExitDialogResponse.Cancel;
					break;
				default:
					throw new Exception("Invalid button");
			}
			Close();
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public enum ExitDialogResponse
	{
		Save,
		Commit,
		Both,
		Exit,
		Cancel
	}
}