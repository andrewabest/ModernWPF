using System;
using System.Timers;
using Caliburn.Micro;

namespace ModernWPF.Client.Features.Alerts
{
    public class AlertModel : PropertyChangedBase, IDisposable
    {
        private readonly Timer _closingTimer;
        private readonly Timer _closeTimer;
        public string Title { get; private set; }
        public string Message { get; private set; }
        public AlertSeverity Severity { get; private set; }
        public event EventHandler<EventArgs> AlertElapsed;
        public DateTimeOffset DateCreated { get; private set; }

        public AlertModel(string title, string message, AlertSeverity severity)
        {
            Title = title;
            Message = message;
            Severity = severity;
            DateCreated = DateTimeOffset.Now;

            _closingTimer = new Timer(5000);
            _closingTimer.Start();
            _closingTimer.Elapsed += (sender, args) =>
            {
                HasElapsed = true;
                _closeTimer.Start();
            };

            _closeTimer = new Timer(1000);
            _closeTimer.Elapsed += (sender, args) => Elapsed();
        }

        public bool HasElapsed { get; set; }

        public void Elapsed()
        {
            HasElapsed = true;

            if (AlertElapsed != null)
                AlertElapsed(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            if (_closeTimer != null)
            {
                _closingTimer.Dispose();
                _closeTimer.Dispose();
            }
        }
    }
}