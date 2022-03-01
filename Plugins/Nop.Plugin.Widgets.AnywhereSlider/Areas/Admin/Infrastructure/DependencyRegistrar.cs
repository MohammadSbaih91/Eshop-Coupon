using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Widgets.AnywhereSlider.Areas.Admin.Factories;

namespace Nop.Plugin.Widgets.AnywhereSlider.Areas.Admin.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<AnywhereSliderFactory>().As<IAnywhereSliderFactory>().InstancePerLifetimeScope();
        }

        public int Order => 1;
    }
}
