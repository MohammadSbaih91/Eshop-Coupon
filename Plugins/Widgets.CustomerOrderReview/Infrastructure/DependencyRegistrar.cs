using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure.Extensions;
using Widgets.CustomerOrderReview.Data;

namespace Widgets.CustomerOrderReview.Infrastructure
{
    /// <summary>
    /// Represents a plugin dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //builder.RegisterType<CustomerOrderReviewCatalogController>().As<CatalogController>().InstancePerLifetimeScope();
            
            //data context
            builder.RegisterPluginDataContext<CustomerOrderReviewObjectContext>("nop_object_customer_order_review");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<Domain.CustomerOrderReview>>().As<IRepository<Domain.CustomerOrderReview>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_customer_order_review"))
                .InstancePerLifetimeScope();
        }

        public int Order => 1;
    }
}