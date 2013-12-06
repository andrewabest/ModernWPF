using Autofac;
using Caliburn.Micro;
using ModernWPF.Client.Features.Application.Customer;

namespace ModernWPF.Client
{
    public class ShellViewModel : LifetimeScopeConductor<Screen>
    {
        public ShellViewModel(
            ILifetimeScope lifetimeScope, 
            IEventAggregatorSubscriptionTracker eventAggregatorSubscriptionTracker) : base(lifetimeScope, eventAggregatorSubscriptionTracker)
        {
        }

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