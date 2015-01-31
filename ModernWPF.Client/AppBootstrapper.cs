using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using ModernWPF.Client.Features.Alerts;

namespace ModernWPF.Client
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            IoC.LetThereBeIoC();
            Alert.EventAggregator = IoC.Container.Resolve<IEventAggregator>();
            Initialize();
        }

        [DebuggerStepThrough]
        protected override object GetInstance(Type service, string key)
        {
            return string.IsNullOrWhiteSpace(key) ? IoC.Container.Resolve(service) : IoC.Container.ResolveNamed(key, service);
        }

        [DebuggerStepThrough]
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return (IEnumerable<object>)IoC.Container.Resolve(typeof(IEnumerable<>).MakeGenericType(service));
        }

        [DebuggerStepThrough]
        protected override void BuildUp(object instance)
        {
            IoC.Container.InjectProperties(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            IoC.Container.Dispose();
        }
    }
}