using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;

namespace ModernWPF.Client.Features.Alerts
{
    public class AlertsViewModel : Conductor<AlertModel>.Collection.AllActive, IHandle<AlertMessage>
    {
        public bool HasAlert
        {
            get { return Items.Any(); }
        }

        protected override void OnActivationProcessed(AlertModel item, bool success)
        {
            base.OnActivationProcessed(item, success);

            NotifyOfPropertyChange(() => HasAlert);

            item.AlertElapsed += (sender, args) => RemoveAlert((AlertModel)sender);
        }

        public void RemoveAlert(AlertModel alert)
        {
            if (Items.Contains(alert))
                DeactivateItem(alert, true);

            alert.Dispose();

            NotifyOfPropertyChange(() => HasAlert);
        }

        public void RemoveSelectedAlert(MouseButtonEventArgs eventArgs)
        {
            var item = (FrameworkElement)eventArgs.OriginalSource;

            RemoveAlert((AlertModel)item.DataContext);
        }

        public void Handle(AlertMessage message)
        {
            ActivateItem(message.Alert);
        }
    }
}