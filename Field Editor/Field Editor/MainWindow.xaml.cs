using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using FieldEditor.CustomAspects;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using FileDialogCustomPlace = Microsoft.Win32.FileDialogCustomPlace;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace FieldEditor
{
	/// <summary>
	///   Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		/// <summary>
		/// A number that sets the minimum number of characters written in the textbox for live searching to be enabled.
		/// </summary>
		private const int MinimumCharactersToAutoSearch = 0;
		public const string Default_NullGroup = "(Ungrouped)";
		public const string Default_Path_Data = "";
		public const string Default_Path_Fields = "fields.xml";
		public const string Default_Path_Settings = "settings.xml";
		private readonly string Path_Application = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
		public event PropertyChangedEventHandler PropertyChanged;

		public MainWindow()
		{
			InitializeComponent();
		}

		public static Kind[] UI_Binding_FieldKinds { get { return (Kind[]) Enum.GetValues(typeof (Kind)); } }

		[ResourceBoundPropertyAspect]
		public ActionCommand_String App_ChangeFolder { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand App_ChangeFolder_Dialog { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand App_Exit_Dialog { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand_String App_OpenFile { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand App_OpenFile_Dialog { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand_String App_SaveAs { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand App_SaveAs_Dialog { get; private set; }

		[NotifyPropertyChangedAspect]
		public Field.Factory Factory { get; set; }

		[NotifyPropertyChangedAspect]
		public string FieldFilePath { get; set; }

		[ResourceBoundPropertyAspect]
		public FieldCollection Fields { get; private set; }

		[ResourceBoundPropertyAspect]
		public CollectionViewSource FieldsViewSource { get { return Resources["cvsFields"] as CollectionViewSource; } }

		[ResourceBoundPropertyAspect]
		public ActionCommand_Fields Fields_Commit { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand_Fields Fields_Delete { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand_Fields Fields_Reload { get; private set; }

		[NotifyPropertyChangedAspect]
		public string Filter { get; set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand Grid_ForceEndEditingMode { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand Grid_Refresh { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand Group_Collapse_All { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand_String Group_Collapse_Others { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand_String Group_Delete { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand Group_Expand_All { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand_String Group_Rename_Dialog { get; private set; }

		[NotifyPropertyChangedAspect]
		public Settings Settings { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand_GroupingIdentifier Subgroup_Delete { get; private set; }

		[ResourceBoundPropertyAspect]
		public ActionCommand_GroupingIdentifier Subgroup_Rename_Dialog { get; private set; }

		public IEnumerable<string> UI_Binding_AutoComplete_FileName { get { return Fields.Select(x => x.FileName).Distinct(); } }

		public IEnumerable<string> UI_Binding_AutoComplete_Group { get { return Fields.Select(x => x.Group).Distinct(); } }

		public IEnumerable<string> UI_Binding_AutoComplete_Subgroup { get { return Fields.Select(x => x.Subgroup).Distinct(); } }

		[NotifyPropertyChangedAspect("FieldFilePath", "Factory")]
		public string UI_MainWindow_Title { get { return "Field Editor - File: [{0}] - Data Folder: [{1}]".FormatWith(FieldFilePath, Factory != null ? Factory.RelativePath : String.Empty); } }

		private void Action_InitializeNewSavedField(Field newField)
		{
			if (Fields.Count <= 1) return;
			var lastField = Fields[Fields.Count - 2];
			if (newField == null) return;
			if (lastField == null) return;
			newField.FileName = lastField.FileName;
			newField.Kind = lastField.Kind;
			newField.Offset = lastField.Offset;
			newField.Group = lastField.Group;
			newField.Subgroup = lastField.Subgroup;
		}

		private void GridOnAddingNewItem(object sender, AddingNewItemEventArgs addingNewItemEventArgs)
		{
			addingNewItemEventArgs.NewItem = Factory.Create();
		}

		/// <summary>
		/// Initializes resource-mapped Command objects.
		/// </summary>
		private void InitializeCommands()
		{
			Grid_ForceEndEditingMode.Execute = () =>
			                                   {
				                                   grid.CommitEdit();
				                                   grid.CancelEdit();
			                                   };
			App_OpenFile_Dialog.Execute = () =>
			                              {
				                              var newPath = ShowDialog_OpenFieldsFile();
				                              if (newPath == null) return;
				                              App_OpenFile.Execute(newPath);
			                              };
			App_Exit_Dialog.Execute = () => Close();
			App_OpenFile.Execute = path =>
			                       {
				                       var loaded = Factory.LoadFieldsFrom(path);
				                       Fields.Clear();
				                       Fields.AddMany(loaded);
				                       FieldFilePath = path;
				                       Settings.Path_MostRecentFile = path;
			                       };

			App_ChangeFolder.Execute = newFolder =>
			                           {
				                           var newFactory = new Field.Factory(newFolder);
				                           var newFields = Fields.Select(newFactory.Clone).ToArray();
				                           Fields.Clear();
				                           newFields.ForEach(x => Fields.Add(x));
				                           Factory = newFactory;
				                           Settings.Path_DataFileFolder = newFolder;
			                           };
			App_SaveAs.Execute = path => Factory.SaveFieldsTo(path, Fields);
			App_ChangeFolder_Dialog.Execute = () =>
			                                  {
				                                  var newFolder = ShowDialog_OpenDataFolder();
				                                  if (newFolder == null) return;
				                                  App_ChangeFolder.Execute(newFolder);
			                                  };

			App_SaveAs_Dialog.Execute = () =>
			                            {
				                            var path = ShowDialog_SaveFieldsFile();
				                            if (path == null) return;
				                            App_SaveAs.Execute(path);
				                            App_OpenFile.Execute(path);
			                            };

			Fields_Commit.Execute = fields => Field.BulkFileOperation(fields, Field.FileAction.Write);
			Fields_Reload.Execute = fields => { Field.BulkFileOperation(fields, Field.FileAction.Read); };
			Fields_Delete.Execute = fields =>
			                        {
				                        var res = ShowDialog_Confirm("Are you sure you want to delete these fields?");
				                        if (!res) return;
				                        Fields.RemoveMany(fields);
			                        };

			Group_Delete.Execute = name => Fields_Delete.Execute(Fields.Where(x => x.Group == name));
			Group_Expand_All.Execute = () => grid.GetVisualChildren<Expander>().ForEach(x => x.IsExpanded = true);
			Group_Collapse_All.Execute = () => grid.GetVisualChildren<Expander>().ForEach(x => x.IsExpanded = false);
			Group_Collapse_Others.Execute = name =>
			                                {
				                                foreach (var expander in grid.GetVisualChildren<Expander>())
				                                {
					                                var grp = expander.DataContext as CollectionViewGroup;
					                                if (!grp.Name.Equals(name)) expander.IsExpanded = false;
				                                }
			                                };
			Subgroup_Delete.Execute = grping => Fields_Delete.Execute(Fields.Where(grping.IsMatch));
			Group_Rename_Dialog.Execute = name =>
			                              {
				                              var result = ShowDialog_Input("Enter a new name for the group \" {0} \":".FormatWith(name),
				                                                            UI_Binding_AutoComplete_Group);
				                              if (result == null) return;

				                              if (Fields.Any(x => x.Group == result))
				                              {
					                              ShowDialog_Error("There already exists a group with that name.");
					                              return;
				                              }
				                              Fields.Where(x => x.Group == name).ForEach(x => x.Group = result);
				                              Grid_ForceEndEditingMode.Execute();
				                              FieldsViewSource.View.Refresh();
			                              };

			Subgroup_Rename_Dialog.Execute = grping =>
			                                 {
				                                 var result =
					                                 ShowDialog_Input(
						                                 "Enter a new name for the subgroup \" {1} \" within group \" {0} \":".FormatWith(grping.Group, grping.Subgroup),
						                                 UI_Binding_AutoComplete_Subgroup);
				                                 if (result == null) return;

				                                 if (Fields.Any(x => x.Group == grping.Group && x.Subgroup == result))
				                                 {
					                                 ShowDialog_Error("This group-subgroup combination already exists.");
					                                 return;
				                                 }
				                                 Fields.Where(grping.IsMatch).ForEach(x => x.Subgroup = result);
				                                 Grid_ForceEndEditingMode.Execute();
				                                 FieldsViewSource.View.Refresh();
			                                 };
		}
		/// <summary>
		/// Returns true if the specified field matches the current filter.
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		private bool IsFilterMatch(Field f)
		{
			if (Filter.IsNullOrEmpty()) return true;
			if (f == null) return false;
			return f.Group.ContainsCapsInsensitive(Filter) || f.Subgroup.ContainsCapsInsensitive(Filter) || f.Name.ContainsCapsInsensitive(Filter);
		}
		/// <summary>
		/// Displays a confirmation dialog box and returns true if the user clicks Yes, and false otherwise.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public bool ShowDialog_Confirm(string text)
		{
			return MessageBox.Show(text, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes ? true : false;
		}
		/// <summary>
		/// Displays an error dialog box with the specified prompt.
		/// </summary>
		/// <param name="text"></param>
		public void ShowDialog_Error(string text)
		{
			MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		/// <summary>
		/// Displays an input dialog box and returns the value the user types, or null if the user cancels.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="dataSource">Source for the textbox's auto completion functionality</param>
		/// <returns></returns>
		public string ShowDialog_Input(string text, IEnumerable dataSource)
		{
			var dlg = new InputBoxPrompt
			          {
				          Title = "Enter input",
				          PromptString = text,
				          AutoCompletionSource = dataSource.Cast<string>(),
				          AutoCompletionEnabled = true
			          };
			var response = dlg.ShowDialog();
			return response == true ? dlg.UserInput : null;
		}
		/// <summary>
		/// Displays a dialog that allows the user to select a folder. Returns the path to the selected folder or null if hte user cancels.
		/// </summary>
		/// <returns></returns>
		public string ShowDialog_OpenDataFolder()
		{
			var dlg = new CommonOpenFileDialog
			          {
				          Multiselect = false,
				          IsFolderPicker = true,
				          Title = "Select a new data folder.",
				          EnsurePathExists = true,
			          };
			var r = dlg.ShowDialog();
			return r == CommonFileDialogResult.Ok ? dlg.FileName : null;
		}
		/// <summary>
		/// Displays a dialog that allows the user to open a field file. Returns the path or null.
		/// </summary>
		/// <returns></returns>
		public string ShowDialog_OpenFieldsFile()
		{
			var dlg = new OpenFileDialog
			          {
				          CheckPathExists = true,
				          CheckFileExists = true,
				          CustomPlaces = new[] {new FileDialogCustomPlace(Path_Application)},
				          Filter = "XML files|*.xml|All files|*.*",
				          DefaultExt = "*.xml",
				          InitialDirectory = Path.GetDirectoryName(FieldFilePath),
				          Title = "Choose a field file to open."
			          };
			var r = dlg.ShowDialog();
			if (r != true) return null;
			return dlg.FileName;
		}
		/// <summary>
		/// Displays a dialog that lets the user save changes. Returns false if the user cancels.
		/// </summary>
		/// <param name="prompt"></param>
		/// <returns></returns>
		private bool ShowDialog_SaveChanges(string prompt = null)
		{
			var dlg = new ExitDialog(prompt ?? "Do you want to save your changes?");
			var res = dlg.ShowDialog();
			switch (dlg.Response)
			{
				case ExitDialogResponse.Cancel:
					return false;
					break;
				case ExitDialogResponse.Commit:
					Fields_Commit.Execute(Fields);
					break;
				case ExitDialogResponse.Save:
					App_SaveAs.Execute(FieldFilePath);
					break;
				case ExitDialogResponse.Both:
					Fields_Commit.Execute(Fields);
					App_SaveAs.Execute(FieldFilePath);
					break;
				case ExitDialogResponse.Exit:
					break;
			}
			return true;
		}
		/// <summary>
		/// Displays a save as dialog. Returns the path selected, or null.
		/// </summary>
		/// <returns></returns>
		public string ShowDialog_SaveFieldsFile()
		{
			var dlg = new SaveFileDialog
			          {
				          CheckPathExists = true,
				          CustomPlaces = new[] {new FileDialogCustomPlace(Path_Application)},
				          Filter = "XML files|*.xml",
				          AddExtension = true,
				          DefaultExt = "*.xml",
				          InitialDirectory = Path.GetDirectoryName(FieldFilePath),
				          Title = "Choose where to save the current field file."
			          };
			var r = dlg.ShowDialog();
			if (r != true) return null;
			return dlg.FileName;
		}

		private void UI_Debug_ClearNotes(object sender, RoutedEventArgs e)
		{
			var r = MessageBox.Show("Sure?", "Urm...", MessageBoxButton.YesNo);
			if (r == MessageBoxResult.No) return;

			foreach (var item in grid.SelectedItems.OfType<Field>())
			{
				item.Notes = "";
			}
		}

		private void UI_FieldCollection_Filter(object sender, FilterEventArgs e)
		{
			var f = e.Item as Field;
			if (f == null) throw new ArgumentException("Item was not of the correct type.");
			e.Accepted = IsFilterMatch(f);
		}

		private void UI_FilterBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter) FieldsViewSource.View.Refresh();
		}

		private void UI_FilterBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (filterTextBox.Text.Length >= MinimumCharactersToAutoSearch)
			{
				Grid_ForceEndEditingMode.Execute();
				FieldsViewSource.View.Refresh();
			}
		}

		private void UI_Grid_InitializeNewItem(object sender, InitializingNewItemEventArgs e)
		{
			Action_InitializeNewSavedField(e.NewItem as Field);
		}

		private void UI_MainWindow_Closed(object sender, EventArgs e)
		{
			Settings.SerializeTo(Default_Path_Settings);
		}

		private void UI_MainWindow_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = !ShowDialog_SaveChanges();
		}

		private void UI_MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			InitializeCommands();
			Settings = Settings.DeserializeOrDefault(Default_Path_Settings);

			var dataFolder = Settings.Path_DataFileFolder;
			if (dataFolder.IsNullOrWhiteSpace() || !Directory.Exists(dataFolder))
				dataFolder = ShowDialog_OpenDataFolder() ?? Default_Path_Data;
			App_ChangeFolder.Execute(dataFolder);
			var fieldsFile = Settings.Path_MostRecentFile;
			if (fieldsFile.IsNullOrWhiteSpace() || !File.Exists(fieldsFile))
				fieldsFile = ShowDialog_OpenFieldsFile() ?? Default_Path_Fields;
			App_OpenFile.Execute(fieldsFile);
			grid.AddingNewItem += GridOnAddingNewItem;
		}
	}
}