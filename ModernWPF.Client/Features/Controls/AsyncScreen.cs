using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Caliburn.Micro;
using ModernWPF.Client.Extensions;

namespace ModernWPF.Client.Features.Controls
{
    public abstract class AsyncScreen : ScreenWithClose, IDisposable, IGetBusy
    {
        private readonly Lazy<WorkQueue> _workQueue;
        private bool _doingBusyWork;
        private bool _isBusy;

        public AsyncScreen()
        {
            _workQueue = new Lazy<WorkQueue>(CreateWorkQueue);
        }

        public virtual bool IsBusy
        {
            get { return _isBusy || _doingBusyWork; }
            set
            {
                _isBusy = value;
                NotifyOfPropertyChange(() => IsBusy);
            }
        }

        private WorkQueue CreateWorkQueue()
        {
            var workQueue = new WorkQueue();
            workQueue.EnteredBusyState += WorkQueueOnEnteredBusyState;
            workQueue.ExitedBusyState += WorkQueueOnExitedBusyState;
            workQueue.ExceptionRaised += WorkQueueOnExceptionRaised;
            return workQueue;
        }

        public void QueueBusyWork(
            Func<Task> action,
            [CallerMemberName] string callerMember = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0,
            Action<Task> continueWith = null)
        {
            var initiatorInfo = InitiatorInfo.Generate(callerMember, callerFile, callerLine);

            _workQueue.Value.QueueBusyWork(action, initiatorInfo, continueWith);
        }

        private void RaiseIsBusyStateEvent()
        {
            var enteredBusyState = IsBusyState;
            if (enteredBusyState != null)
                enteredBusyState(this, EventArgs.Empty);
        }

        private void RaiseIsNoLongerBusyStateEvent()
        {
            var exitedBusyState = IsNoLongerBusyState;
            if (exitedBusyState != null)
                exitedBusyState(this, EventArgs.Empty);
        }

        private void WorkQueueOnEnteredBusyState(object sender, EventArgs e)
        {
            _doingBusyWork = true;
            RaiseIsBusyStateEvent();
            NotifyOfPropertyChange(() => IsBusy);
        }

        private void WorkQueueOnExitedBusyState(object sender, EventArgs e)
        {
            _doingBusyWork = false;
            RaiseIsNoLongerBusyStateEvent();
            NotifyOfPropertyChange(() => IsBusy);
        }

        private void WorkQueueOnExceptionRaised(object sender, AggregateException aggregateException)
        {
            foreach (var exception in aggregateException.InnerExceptions)
            {
                var temp = exception;
                if (temp is ObjectDisposedException)
                {
                    Serilog.Log.Error("Object disposed exception caught in the work queue: {Message}", temp, temp.Message);
                    return;
                }
                Execute.OnUIThread(() => { throw new Exception("See inner exception", temp); });
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (_workQueue.IsValueCreated)
            {
                _workQueue.Value.EnteredBusyState -= WorkQueueOnEnteredBusyState;
                _workQueue.Value.ExitedBusyState -= WorkQueueOnExitedBusyState;
                _workQueue.Value.Dispose();
            }
        }

        protected async Task TryCloseAsync(Func<Task> asyncMethod)
        {
            try
            {
                await asyncMethod();
            }
            catch (Exception ex)
            {
                Execute.BeginOnUIThread(() => { throw new Exception("Failed executing {0}: {1}".FormatWith(GetType().Name, ex.Message), ex); });
            }
        }

        public event EventHandler<EventArgs> IsBusyState;
        public event EventHandler<EventArgs> IsNoLongerBusyState;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}