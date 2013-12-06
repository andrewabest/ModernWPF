using System;
using Autofac;
using Serilog;

namespace ModernWPF.Client.AutofacModules
{
    public class LoggingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ILogger>()
                .OnActivating(e => 
                    e.ReplaceInstance(
                        new LoggerConfiguration().WriteTo.RollingFile(Environment.CurrentDirectory).CreateLogger()))
                .SingleInstance();
        }
    }
}