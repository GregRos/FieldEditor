using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace FieldEditor
{
	public class ActionCommand<T> : ICommand
	{
		public static readonly ActionCommand<T> Default = new ActionCommand<T>();
		public event EventHandler CanExecuteChanged;

		public static implicit operator ActionCommand<T>(Action<T> act)
		{
			return new ActionCommand<T>
			       {
				       Execute = act
			       };
		}

		public Func<T, bool> CanExecute { get; set; }
		public Action<T> Execute { get; set; }

		bool ICommand.CanExecute(object parameter)
		{
			return CanExecute == null || CanExecute((T) parameter);
		}

		void ICommand.Execute(object parameter)
		{
			if (Execute != null) Execute((T) parameter);
		}
	}

	public class ActionCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;

		public ActionCommand()
		{
		}

		public Func<bool> CanExecute { get; set; }
		public Action Execute { get; set; }

		bool ICommand.CanExecute(object parameter)
		{
			return CanExecute == null ? true : CanExecute();
		}

		void ICommand.Execute(object parameter)
		{
			if (Execute != null) Execute();
		}
	}

	public class ActionCommand_Expander : ActionCommand<Expander>
	{
	}

	public class ActionCommand_String : ActionCommand<string>
	{
	}

	public class ActionCommand_Fields : ActionCommand<IEnumerable<Field>>
	{
	}

	public class ActionCommand_GroupingIdentifier : ActionCommand<Field.GroupingIdentifier>
	{
	}
}