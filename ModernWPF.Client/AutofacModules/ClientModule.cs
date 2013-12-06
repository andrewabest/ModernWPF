using Autofac;
using Caliburn.Micro;

namespace ModernWPF.Client.AutofacModules
{
    public class ClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowManager>().AsImplementedInterfaces().InstancePerDependency();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(a => a.Name.EndsWith("View"))
                .AsSelf()
                .InstancePerDependency();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Name.EndsWith("ViewModel"))
                .AsSelf()
                .InstancePerDependency();
        }
    }
}