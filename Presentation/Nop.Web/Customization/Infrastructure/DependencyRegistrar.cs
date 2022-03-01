using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Domain.Employees;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Services.Card;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customization.Catalog;
using Nop.Services.Customization.Discounts;
using Nop.Services.Customization.Orders;
using Nop.Services.Customization.Seo;
using Nop.Services.Customization.Tasks;
using Nop.Services.Employees;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Tasks;
using Nop.Web.Areas.Admin.Customization.Factories;
using Nop.Web.Customization.Factories;
using Nop.Web.Factories;
using Nop.Web.Framework.Infrastructure.Extensions;
using AdminFactories = Nop.Web.Areas.Admin.Factories;

namespace Nop.Web.Customization.infrastructure
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<SpecificationAttributeGroupModelFactory>().As<ISpecificationAttributeGroupModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ShoppingCartModelFactoryOverride>().As<IShoppingCartModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ProductModelFactoryOverride>().As<IProductModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AddressModelFactoryOverride>().As<IAddressModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<OrderProcessingServiceOverride>().As<IOrderProcessingService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderProcessingServiceOverride>().As<IOrderProcessingServiceOverride>().InstancePerLifetimeScope();
            builder.RegisterType<CommonModelFactoryOverride>().As<ICommonModelFactory>().InstancePerLifetimeScope();
            
            builder.RegisterType<AdminFactories.ProductModelFactoryOverride>().As<AdminFactories.IProductModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AdminFactories.CustomerModelFactoryOverride>().As<AdminFactories.ICustomerModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AdminFactories.OrderModelFactoryOverride>().As<AdminFactories.IOrderModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AdminFactories.ShoppingCartModelFactoryOverride>().As<AdminFactories.IShoppingCartModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AdminFactories.ReportModelFactoryOverride>().As<AdminFactories.IReportModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AdminFactories.EmployeeOrderModelFactory>().As<AdminFactories.IEmployeeOrderModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AdminFactories.SpecificationAttributeGroupModelFactory>().As<AdminFactories.ISpecificationAttributeGroupModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AdminFactories.OverrideSpecificationAttributeModelFactory>().As<AdminFactories.ISpecificationAttributeModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AdminFactories.NewsLetterSubscriptionTypeModelFactory>().As<AdminFactories.INewsLetterSubscriptionTypeModelFactory>().InstancePerLifetimeScope();

            builder.RegisterType<CustomReportModelFactory>().As<ICustomReportModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<CustomeProductModelFactory>().As<ICustomeProductModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<CatalogModelFactoryOverride>().As<ICatalogModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<OrderModelFactoryOverride>().As<IOrderModelFactory>().InstancePerLifetimeScope();

            builder.RegisterType<EmployeeService>().As<IEmployeeService>().InstancePerLifetimeScope();
            builder.RegisterType<SimCardService>().As<ISimCardService>().InstancePerLifetimeScope();
            builder.RegisterType<PackagesService>().As<IPackagesService>().InstancePerLifetimeScope();
            builder.RegisterType<AdminFactories.PackagesFactory>().As<AdminFactories.IPackagesFactory>().InstancePerLifetimeScope();
            builder.RegisterType<PackageProductService>().As<IPackageProductService>().InstancePerLifetimeScope();

            builder.RegisterType<OrderTotalCalculationServiceOverride>().As<IOrderTotalCalculationService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductAttributeFormatterOverride>().As<IProductAttributeFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<CustomeOrderService>().As<ICustomeOrderService>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleTaskServiceOverride>().As<IScheduleTaskService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomDiscountService>().As<ICustomDiscountService>().InstancePerLifetimeScope();
            builder.RegisterType<OverrideCompareProductsService>().As<ICompareProductsService>().InstancePerLifetimeScope();
            builder.RegisterType<CheckoutModelFactoryOverride>().As<ICheckoutModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<CategoryProductBoxTemplateService>().As<ICategoryProductBoxTemplateService>().InstancePerLifetimeScope();

            //admin Factories and service
            builder.RegisterType<Nop.Web.Areas.Admin.Customization.Factories.OverrideCategoryModelFactory>().As<Nop.Web.Areas.Admin.Factories.ICategoryModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<Nop.Web.Areas.Admin.Factories.CustomeBaseAdminModelFactory>().As<Nop.Web.Areas.Admin.Factories.ICustomeBaseAdminModelFactory>().InstancePerLifetimeScope();

            #region Nop.Web.Framework.DependencyRegistrar

            builder.RegisterType<PriceCalculationServiceOverride>().As<IPriceCalculationService>().InstancePerLifetimeScope();
            builder.RegisterType<PdfServiceOverride>().As<IPdfService>().InstancePerLifetimeScope();
            //builder.RegisterType<ShoppingCartServiceOverride>().As<IShoppingCartService>().InstancePerLifetimeScope();
            builder.RegisterType<PictureServiceOverride>().As<IPictureService>().InstancePerLifetimeScope();
            //file provider
            builder.RegisterType<NopFileProviderOverride>().Named<INopFileProvider>("Images").InstancePerLifetimeScope();
            builder.RegisterType<SitemapGeneratorOverride>().As<ISitemapGenerator>().InstancePerLifetimeScope();

            builder.RegisterType<SpecificationAttributeGroupService>().As<ISpecificationAttributeGroupService>().InstancePerLifetimeScope();
            builder.RegisterType<NewsLetterSubscriptionTypeService>().As<INewsLetterSubscriptionTypeService>().InstancePerLifetimeScope();
            #endregion

            ////override required repository with our custom context
            //builder.RegisterType<EfRepository<Employee>>().As<IRepository<Employee>>()
            //    .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context"))
            //    .InstancePerLifetimeScope();


        }

        /// <summary>
        /// Gets order of this dependency registrar implementation
        /// </summary>
        public int Order => 99;
    }
}