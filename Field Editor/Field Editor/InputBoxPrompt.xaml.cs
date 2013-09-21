using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using FieldEditor.CustomAspects;

namespace FieldEditor
{
	/// <summary>
	///   Interaction logic for InputBoxPrompt.xaml
	/// </summary>
	public partial class InputBoxPrompt : Window, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public InputBoxPrompt()
		{
			InitializeComponent();
		}

		[NotifyPropertyChangedAspect]
		public bool AutoCompletionEnabled { get; set; }

		[NotifyPropertyChangedAspect]
		public IEnumerable<string> AutoCompletionSource { get; set; }

		[NotifyPropertyChangedAspect]
		public string PromptString { get; set; }

		[NotifyPropertyChangedAspect]
		public string UserInput { get; private set; }

		private void UI_Button_Cancel(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void UI_Button_OK(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}

		private void Window_Loaded_1(object sender, RoutedEventArgs e)
		{
			boxInput.Focusable = true;
			var x = boxInput.Focus();
		}
	}
}