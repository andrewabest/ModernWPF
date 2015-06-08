using System;
using System.Windows.Input;

namespace ModernWPF.Client.Features.Actions
{
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> execute)
            : this(execute, (Predicate<object>)null)
        {
        }

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public void RaiseCanExecuteChanged()
        {
            if (this.CanExecuteChanged == null)
                return;
            this.CanExecuteChanged((object)this, EventArgs.Empty);
        }

        public virtual bool CanExecute(object parameter)
        {
            if (this._canExecute == null)
                return true;
            return this._canExecute(parameter);
        }

        public virtual void Execute(object parameter)
        {
            this._execute(parameter);
        }
    }
}