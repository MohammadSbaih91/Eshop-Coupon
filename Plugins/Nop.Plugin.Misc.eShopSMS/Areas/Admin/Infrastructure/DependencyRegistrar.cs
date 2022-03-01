using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Misc.eShopSMS.Areas.Admin.Factories;

namespace Nop.Plugin.Misc.AppointmentBooking.Areas.Admin.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<SmsTemplateModelFactory>().As<ISmsTemplateModelFactory>().InstancePerLifetimeScope();
        }

        public int Order => 1;
    }
}
