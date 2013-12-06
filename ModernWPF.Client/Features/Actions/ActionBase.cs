using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using ModernWPF.Client.Extensions;

namespace ModernWPF.Client.Features.Actions
{
    using CalExec = global::Caliburn.Micro.Execute;

    public abstract class ActionBase : PropertyChangedBase, ICommand
    {
        protected ActionBase()
        {
        }

        public abstract void Execute(object parameter);

        protected void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CalExec.OnUIThread(() => CanExecuteChanged(this, EventArgs.Empty));
        }

        public virtual string ToolTip
        {
            get
            {
                var toolTip = string.Empty;
                var failureReasons = CanExecuteFailedReasons.ToArray();
                if (failureReasons.Any())
                {
                    toolTip += Environment.NewLine;
                    toolTip += string.Join(Environment.NewLine, failureReasons);
                }

                return toolTip;
            }
        }

        public virtual bool CanExecute { get { return CanExecuteFailedReasons.None(); } }

        public virtual IEnumerable<string> CanExecuteFailedReasons { get { return Enumerable.Empty<string>(); } }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute;
        }

        public event EventHandler CanExecuteChanged;
    }
}