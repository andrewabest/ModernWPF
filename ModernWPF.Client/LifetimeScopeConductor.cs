using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;

namespace ModernWPF.Client
{
    public abstract class LifetimeScopeConductor<T> : Conductor<T>.Collection.OneActive where T : class, IScreen
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IEventAggregatorSubscriptionTracker _eventAggregatorSubscriptionTracker;
        private readonly Dictionary<T, ILifetimeScope> _lifetimeScopes;
        private readonly Func<Type, Func<T, bool>> _defaultItemPredicate = comparisonType => item => item.GetType() == comparisonType;

        protected abstract bool ShouldDeactivatePreviouslyActiveItems { get; }

        protected LifetimeScopeConductor(ILifetimeScope lifetimeScope, IEventAggregatorSubscriptionTracker eventAggregatorSubscriptionTracker)
        {
            _lifetimeScope = lifetimeScope;
            _eventAggregatorSubscriptionTracker = eventAggregatorSubscriptionTracker;
            _lifetimeScopes = new Dictionary<T, ILifetimeScope>();
        }

        protected void Activate<TU>() where TU : T
        {
            Activate(typeof(TU));
        }

        protected void Activate<TU>(IEnumerable<KeyValuePair<string, object>> parameters) where TU : T
        {
            Activate(typeof(TU), null, parameters);
        }

        protected void Activate<TU>(Func<T, bool> itemPredicate, IEnumerable<KeyValuePair<string, object>> parameters) where TU : T
        {
            Activate(typeof(TU), itemPredicate, parameters);
        }

        protected void Activate(Type screenType, Func<T, bool> itemPredicate = null, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            var predicate = itemPredicate ?? _defaultItemPredicate(screenType);
            var previouslyActivatedItem = Items.SingleOrDefault(predicate);
            if (previouslyActivatedItem != null)
            {
                ActivateItem(previouslyActivatedItem);
                return;
            }

            var resolutionParameters = parameters ?? Enumerable.Empty<KeyValuePair<string, object>>();

            if (ShouldDeactivatePreviouslyActiveItems && ActiveItem != null)
            {
                Deactivate(ActiveItem);
            }

            var viewModelLifetimeScope = _lifetimeScope.BeginLifetimeScope(screenType);

            var target = viewModelLifetimeScope.Resolve(screenType, resolutionParameters.Select(p => new NamedParameter(p.Key, p.Value)));

            var viewModel = target as T;
            if (viewModel == null)
                throw new InvalidOperationException("Type supplied must implement IScreen");

            _lifetimeScopes.Add(viewModel, viewModelLifetimeScope);

            ActivateItem(viewModel);
        }

        private void Deactivate(T workspace)
        {
            DeactivateItem(workspace, true);

            if (_lifetimeScopes.ContainsKey(workspace))
            {
                var lifetimeScope = _lifetimeScopes[workspace];
                _eventAggregatorSubscriptionTracker.EndScope(lifetimeScope);
                lifetimeScope.Dispose();
            }
        }
    }
}