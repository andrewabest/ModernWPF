using System.Collections.Generic;
using Caliburn.Micro;

namespace ModernWPF.Client
{
    public class EventAggregatorSubscriptionTracker : IEventAggregatorSubscriptionTracker
    {
        private readonly IEventAggregator _messageBus;
        private readonly object _updateLock = new object();

        // keyed on scope, contains a list of instances with subscriptions.
        private readonly IDictionary<object, List<IHandle>> _instances;

        public EventAggregatorSubscriptionTracker(IEventAggregator messageBus)
        {
            _messageBus = messageBus;
            _instances = new Dictionary<object, List<IHandle>>();
        }

        public void SubscribeAndTrack(object scope, IHandle instance)
        {
            _messageBus.Subscribe(instance);

            lock (_updateLock)
            {
                List<IHandle> scopeInstances;
                if (_instances.TryGetValue(scope, out scopeInstances) == false)
                {
                    scopeInstances = new List<IHandle>();
                    _instances.Add(scope, scopeInstances);
                }

                if (scopeInstances.Contains(instance) == false)
                    scopeInstances.Add(instance);
            }
        }

        public void EndScope(object scope)
        {
            lock (_updateLock)
            {
                List<IHandle> scopeInstances;
                if (!_instances.TryGetValue(scope, out scopeInstances))
                    return;

                foreach (var instance in scopeInstances)
                {
                    _messageBus.Unsubscribe(instance);
                }

                scopeInstances.Clear();
                _instances.Remove(scope);
            }
        }
    }
}