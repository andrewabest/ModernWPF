using Caliburn.Micro;

namespace ModernWPF.Client
{
    public interface IEventAggregatorSubscriptionTracker
    {
        void SubscribeAndTrack(object scope, IHandle instance);
        void EndScope(object scope);
    }
}