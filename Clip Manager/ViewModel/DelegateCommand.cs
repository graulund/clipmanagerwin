using System;
using System.Windows.Input;

namespace Clip_Manager.ViewModel
{
	class DelegateCommand : ICommand
	{
		private readonly Action<object> action;
		private bool isEnabled;

		public DelegateCommand(Action<object> action)
		{
			this.action = action;
			isEnabled = true;
		}

		public void Execute(object parameter)
		{
			action(parameter);
		}

		public bool CanExecute(object parameter)
		{
			return isEnabled;
		}

		public bool IsEnabled
		{
			get
			{
				return isEnabled;
			}
			set
			{
				if (isEnabled != value)
				{
					isEnabled = value;
					OnCanExecuteChanged();
				}
			}
		}

		public event EventHandler CanExecuteChanged;

		protected virtual void OnCanExecuteChanged()
		{
			EventHandler handler = CanExecuteChanged;
			if (handler != null) handler(this, EventArgs.Empty);
		}
	}
}
