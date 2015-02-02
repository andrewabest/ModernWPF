using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace ModernWPF.Client.Features.Controls
{
    public class WorkQueue : IDisposable
    {
        public event EventHandler<EventArgs> EnteredBusyState;
        public event EventHandler<AggregateException> ExceptionRaised;
        public event EventHandler<EventArgs> ExitingTask;
        public event EventHandler<EventArgs> ExitedBusyState;

        private readonly ConcurrentQueue<WorkQueueItem> _tasksToRun = new ConcurrentQueue<WorkQueueItem>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private bool _disposed;

        public void QueueBusyWork(Func<Task> taskFunc, string initiatorInfo = null, Action<Task> continueWith = null)
        {
            if (_disposed)
                return;

            if (taskFunc == null)
                return;

            var item = new WorkQueueItem(taskFunc, initiatorInfo, continueWith);

            _tasksToRun.Enqueue(item);
            StartWorkerThread();
        }

        private void StartWorkerThread()
        {
            Task.Run(() => DoBusyWork())
                .ContinueWith(x => RaiseExitingTaskEvent());
        }

        private void RaiseExitingTaskEvent()
        {
            var exitingTask = ExitingTask;
            if (exitingTask != null)
                exitingTask(this, EventArgs.Empty);
        }

        private void RaiseExceptionRaisedEvent(AggregateException exception)
        {
            var eventHandler = ExceptionRaised;
            if (eventHandler != null)
                eventHandler(this, exception);
        }

        private async Task DoBusyWork()
        {
            if (_semaphore.Wait(0) == false)
                return;

            try
            {
                RaiseEnteredBusyStateEvent();

                while (true)
                {
                    if (_disposed)
                        break;

                    WorkQueueItem workItem;
                    if (_tasksToRun.TryDequeue(out workItem) == false)
                        break;

                    var task = workItem.TaskFunc();
                    var continueWith = task.ContinueWith(workItem.ContinueWith ?? new Action<Task>(_ => { }));
                    var completed = task.ContinueWith(HandleCompletion, TaskContinuationOptions.NotOnFaulted);
                    var exception = task.ContinueWith(HandleException, TaskContinuationOptions.OnlyOnFaulted);

                    using (LogContext.PushProperty("Initiator", workItem.InitiatorInfo))
                    using (Log.Logger.BeginTimedOperation("QueueBusyWork iniatiated by " + workItem.InitiatorInfo, level: LogEventLevel.Debug, warnIfExceeds: TimeSpan.FromSeconds(10)))
                    {
                        await continueWith;
                        await Task.WhenAny(completed, exception);
                    }
                }

                RaiseExitedBusyStateEvent();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void HandleException(Task task)
        {
            StartWorkerThread();
            RaiseExceptionRaisedEvent(task.Exception);
        }

        private void RaiseEnteredBusyStateEvent()
        {
            var enteredBusyState = EnteredBusyState;
            if (enteredBusyState != null)
                enteredBusyState(this, EventArgs.Empty);
        }

        private void RaiseExitedBusyStateEvent()
        {
            var exitedBusyState = ExitedBusyState;
            if (exitedBusyState != null)
                exitedBusyState(this, EventArgs.Empty);
        }

        private void HandleCompletion(Task task)
        {
            if (_tasksToRun.Any())
                StartWorkerThread();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            // _semaphore.Dispose(); Don't explicitly dispose, this can cause issues in the UI, let the GC clean it up later
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}