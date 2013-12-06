using Autofac;

namespace ModernWPF.Client.AutofacModules
{
    public class ActionsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t =>
                    t.Namespace.Contains("Actions")
                )
                .AsSelf()
                .InstancePerDependency();
        }
    }
}