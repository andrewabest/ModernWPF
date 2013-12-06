using Autofac;
using Caliburn.Micro;
using ModernWPF.Client.Features.Alerts;
using ModernWPF.Client.Features.Application.Customer;

namespace ModernWPF.Client
{
    public class ShellViewModel : LifetimeScopeConductor<Screen>
    {
        private readonly AlertsViewModel _alerts;

        public ShellViewModel(
            ILifetimeScope lifetimeScope, 
            IEventAggregatorSubscriptionTracker eventAggregatorSubscriptionTracker,
            AlertsViewModel alerts) : base(lifetimeScope, eventAggregatorSubscriptionTracker)
        {
            _alerts = alerts;
        }

        public AlertsViewModel Alerts { get { return _alerts; } }

        protected override bool ShouldDeactivatePreviouslyActiveItems
        {
            get { return true; }
        }

        protected override void OnActivate()
        {
            Activate<CustomerViewModel>();
        }
    }
}