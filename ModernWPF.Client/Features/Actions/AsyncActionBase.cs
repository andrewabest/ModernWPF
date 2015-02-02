using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using ModernWPF.Client.Extensions;
using Serilog;
using Serilog.Context;

namespace ModernWPF.Client.Features.Actions
{
    public abstract class AsyncActionBase : PropertyChangedBase, ICommandEx
    {
        bool ICommand.CanExecute(object parameter)
        {
            return IsExecuting == false && CanExecute;
        }

        public async void Execute(object parameter)
        {
            await ExecuteTask(parameter);
        }

        public async Task ExecuteTask(object parameter)
        {
            if (CanExecute == false)
                return;

            using (LogContext.PushProperty("Initiator", GetType().Name))
            using (Log.Logger.BeginTimedOperation("Executing " + GetType().Name, warnIfExceeds: TimeSpan.FromSeconds(5)))
            {
                IsExecuting = true;
                NotifyOfPropertyChange(() => IsExecuting);
                RaiseCanExecuteChanged();

                try
                {
                    await ExecuteAsync(parameter);
                }
                catch (Exception ex)
                {
                    var args = new AsyncActionErrorArgs();
                    ExecuteOnError(ex, args);
                    if (!args.IsHandled)
                        Caliburn.Micro.Execute.BeginOnUIThread(() => { throw new Exception("Failed executing {0}: {1}".FormatWith(GetType().Name, ex.Message), ex); });
                }
                finally
                {
                    IsExecuting = false;
                    NotifyOfPropertyChange(() => IsExecuting);
                    RaiseCanExecuteChanged();
                }
            }
        }

        protected abstract Task ExecuteAsync(object parameter);

        protected virtual void ExecuteOnError(Exception e, AsyncActionErrorArgs args) { }

        protected void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                Caliburn.Micro.Execute.OnUIThread(() => CanExecuteChanged(this, EventArgs.Empty));
        }

        public virtual string ToolTip
        {
            get
            {
                var toolTip = string.Empty;
                var failureReasons = CanExecuteFailedReasons.ToArray();
                if (failureReasons.Any())
                {
                    toolTip += string.Join(Environment.NewLine, failureReasons);
                }

                return toolTip;
            }
        }

        public bool IsExecuting { get; protected set; }

        public virtual bool CanExecute { get { return CanExecuteFailedReasons.None(); } }

        public virtual IEnumerable<string> CanExecuteFailedReasons { get { return Enumerable.Empty<string>(); } }

        public event EventHandler CanExecuteChanged;
    }
}