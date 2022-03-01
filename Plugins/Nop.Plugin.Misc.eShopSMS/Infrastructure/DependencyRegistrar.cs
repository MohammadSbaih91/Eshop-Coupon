using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Misc.eShopSMS.Data;
using Nop.Plugin.Misc.eShopSMS.Domains;
using Nop.Plugin.Misc.eShopSMS.Services;
using Nop.Services.Orders;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Misc.eShopSMS.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private static string CONTEXT_NAME = "Nop.Plugin.eShopSMS_object_context";

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<SmsTemplateService>().As<ISmsTemplateService>().InstancePerLifetimeScope();
            builder.RegisterType<SendSMSNotificationService>().As<ISendSMSNotificationService>().InstancePerLifetimeScope();
            builder.RegisterType<OverrideOrderProcessingService>().As<IOrderProcessingService>().InstancePerLifetimeScope();

            //builder.RegisterType<CustomModelFactory>().As<ICustomModelFactory>().InstancePerLifetimeScope();

            //data context
            builder.RegisterPluginDataContext<SMSObjectContext>(CONTEXT_NAME);

            //override required repository with our custom context
            builder.RegisterType<EfRepository<SMSTemplate>>().As<IRepository<SMSTemplate>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
                .InstancePerLifetimeScope();
        }

        public int Order => 33;
    }
}
