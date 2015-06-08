using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using ModernWPF.Client.Features.Alerts;
using Serilog.Events;

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
            Application.Exit += Application_Exit;

            DisplayRootViewFor<ShellViewModel>();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            IoC.Container.Dispose();
        }

        void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                // SetLogLevel(LogEventLevel.Debug);

                var cleanupTask = Task.Run(() =>
                {
                    var container = IoC.Container;
                    if (container != null)
                    {
                        container.Dispose();
                    }
                });

                var waitBeforeKillingDead = TimeSpan.FromSeconds(30);
                var killShot = Task.Delay(waitBeforeKillingDead).ContinueWith(x =>
                {
                    Serilog.Log.Error("The shut down process took longer than {waitTime}. Killing the process dead.", waitBeforeKillingDead);
                    ForceExit();
                });

                Task.WaitAny(cleanupTask, killShot);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "An exception was thrown when shutting down the application: {message}", ex.Message);
                ForceExit();
            }
        }

        private void ForceExit()
        {
            Serilog.Log.Error("The application was forced to exit.");
            Thread.Sleep(TimeSpan.FromSeconds(5)); // Give the logs time to flush.
            Environment.Exit(0);
        }
    }
}