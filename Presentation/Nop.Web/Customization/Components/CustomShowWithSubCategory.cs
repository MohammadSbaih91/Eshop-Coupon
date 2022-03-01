using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using System.Linq;


namespace Nop.Web.Components
{
    public class CustomShowWithSubCategoryViewComponent : NopViewComponent
    {
        #region Fields
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IWebHelper _webHelper;
        private readonly MediaSettings _mediaSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        #endregion

        #region Ctor
        public CustomShowWithSubCategoryViewComponent(ICatalogModelFactory catalogModelFactory,
            IWebHelper webHelper,
            MediaSettings mediaSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            IStaticCacheManager cacheManager,
            ICategoryService categoryService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService)
        {
            _catalogModelFactory = catalogModelFactory;
            _webHelper = webHelper;
            _mediaSettings = mediaSettings;
            _workContext = workContext;
            _storeContext = storeContext;
            _cacheManager = cacheManager;
            _categoryService = categoryService;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;

        }
        #endregion

        #region Methods
        public IViewComponentResult Invoke()
        {
            var model = _catalogModelFactory.PrepareShowWithSubCategoryModels();

            if (!model.Any())
                return Content("");

            if (model != null && model.Count() > 0)
            {
                foreach (var item in model)
                {
                    var pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var subCategoriesCacheKey = string.Format(EShopHelper.CacheKey_CategoryHomepageSubcategories,
                        item.Id,
                        pictureSize,
                        string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                        _storeContext.CurrentStore.Id,
                        _workContext.WorkingLanguage.Id,
                        _webHelper.IsCurrentConnectionSecured());

                        item.SubCategories = _cacheManager.Get(subCategoriesCacheKey, () =>
                        _categoryService.GetAllCategoriesByParentCategoryId(item.Id)
                        .Select(x =>
                        {
                            var subCatModel = new CategoryModel.SubCategoryModel
                            {
                                Id = x.Id,
                                Name = _localizationService.GetLocalized(x, y => y.Name),
                                SeName = _urlRecordService.GetSeName(x),
                                Description = _localizationService.GetLocalized(x, y => y.Description)
                            };
                            return subCatModel;
                        }).ToList());
                }
            }
            return View(model);
        }
        #endregion
    }
}
