using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Web.Areas.Admin.Customization.Factories
{
    public partial class OverrideCategoryModelFactory : CategoryModelFactory
    {
        #region Fields

        private readonly ICustomeBaseAdminModelFactory _customeBaseAdminModelFactory;
        
        #endregion

        public OverrideCategoryModelFactory(CatalogSettings catalogSettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            IDiscountService discountService,
            IDiscountSupportedModelFactory discountSupportedModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IProductService productService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IUrlRecordService urlRecordService,
            ICustomeBaseAdminModelFactory customeBaseAdminModelFactory) :base(catalogSettings,
            aclSupportedModelFactory,
            baseAdminModelFactory,
            categoryService,
            discountService,
            discountSupportedModelFactory,
            localizationService,
            localizedModelFactory,
            productService,
            storeMappingSupportedModelFactory,
            urlRecordService)
        {
            _customeBaseAdminModelFactory = customeBaseAdminModelFactory;
        }

        #region Method
        public override CategoryModel PrepareCategoryModel(CategoryModel model, Category category, bool excludeProperties = false)
        {
            var categorymodel = base.PrepareCategoryModel(model, category, excludeProperties);

            //prepare available category templates
            _customeBaseAdminModelFactory.PrepareCategoryProductBoxTemplate(categorymodel.AvailableCategoryProductBoxTemplates, false);

            return categorymodel;
        }
        #endregion
    }
}
