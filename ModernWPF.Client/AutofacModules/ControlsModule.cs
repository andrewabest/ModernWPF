using Autofac;
using ModernWPF.Client.Features.Controls;

namespace ModernWPF.Client.AutofacModules
{
    public class ControlsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DialogConductorViewModel>()
                .As<IDialogConductor>()
                .SingleInstance();
        }
    }
}