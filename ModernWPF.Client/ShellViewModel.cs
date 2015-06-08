using Autofac;
using Caliburn.Micro;
using ModernWPF.Client.Features.Alerts;
using ModernWPF.Client.Features.Application.Customer;
using ModernWPF.Client.Features.Controls;

namespace ModernWPF.Client
{
    public class ShellViewModel : LifetimeScopeConductor<Screen>
    {
        private readonly AlertsViewModel _alerts;
        private readonly IDialogConductor _dialogConductor;

        public ShellViewModel(
            ILifetimeScope lifetimeScope, 
            IEventAggregatorSubscriptionTracker eventAggregatorSubscriptionTracker,
            AlertsViewModel alerts,
            IDialogConductor dialogConductor) : base(lifetimeScope, eventAggregatorSubscriptionTracker)
        {
            _alerts = alerts;
            _dialogConductor = dialogConductor;
        }

        public AlertsViewModel Alerts { get { return _alerts; } }

        protected override bool ShouldDeactivatePreviouslyActiveItems
        {
            get { return true; }
        }

        public IDialogConductor DialogConductor
        {
            get { return _dialogConductor; }
        }

        protected override void OnActivate()
        {
            Activate<CustomerViewModel>();
        }
    }
}