using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac;
using Caliburn.Micro;

namespace ModernWPF.Client
{
    public class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
        public AppBootstrapper()
        {
            IoC.LetThereBeIoC();
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

        protected override void OnExit(object sender, EventArgs e)
        {
            IoC.Container.Dispose();
        }
    }
}