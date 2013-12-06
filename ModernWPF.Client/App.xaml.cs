using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using ModernWPF.Client.Extensions;
using Serilog;
using Serilog.Events;

namespace ModernWPF.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = IoC.Container.Resolve<ILogger>();
            var exception = e.ExceptionObject as Exception;

            if (exception == null && e.ExceptionObject != null)
            {
                exception = new ApplicationException(">A non-CLI exception occurred: {0}".FormatWith(e.ExceptionObject));
            }

            logger.Write(LogEventLevel.Fatal, exception, "An unhandled exception occurred", true);
        }

        public static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var logger = IoC.Container.Resolve<ILogger>();

            logger.Write(LogEventLevel.Fatal, e.Exception, "An unhandled exception occurred", true);
        }
    }
}
