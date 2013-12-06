using Autofac;
using Autofac.Core;
using Autofac.Core.Resolving;
using Caliburn.Micro;

namespace ModernWPF.Client.AutofacModules
{
    public class MessagingModule : Module
    {
        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            base.AttachToComponentRegistration(componentRegistry, registration);

            if (registration.Activator.LimitType.IsAssignableTo<IHandle>())
            {
                registration.Activated += RegistrationOnActivated;
            }
        }

        private void RegistrationOnActivated(object sender, ActivatedEventArgs<object> e)
        {
            WireUpMessageHandlers(e);
        }

        private static void WireUpMessageHandlers(ActivatedEventArgs<object> e)
        {
            var handler = e.Instance as IHandle;
            if (handler == null)
                return;

            var tracker = e.Context.Resolve<IEventAggregatorSubscriptionTracker>();
            tracker.SubscribeAndTrack(((IInstanceLookup)e.Context).ActivationScope, handler);
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EventAggregator>()
                .As<IEventAggregator>()
                .SingleInstance();

            builder.RegisterType<EventAggregatorSubscriptionTracker>()
                .As<IEventAggregatorSubscriptionTracker>()
                .SingleInstance();
        }
    }
}