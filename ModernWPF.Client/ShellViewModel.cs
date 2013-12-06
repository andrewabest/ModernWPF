using Autofac;
using Caliburn.Micro;

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


    }
}