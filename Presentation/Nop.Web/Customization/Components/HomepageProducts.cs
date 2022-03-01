using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Customization.Models.Catalog;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Components
{
    public class HomepageProductsViewComponent : NopViewComponent
    {
        private readonly IWebHelper _webHelper;
        private readonly MediaSettings _mediaSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICatalogModelFactory _catalogModelFactory;


        public HomepageProductsViewComponent(IWebHelper webHelper,
            MediaSettings mediaSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            IStaticCacheManager cacheManager,
            ICategoryService categoryService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            ICatalogModelFactory catalogModelFactory)
        {
            this._webHelper = webHelper;
            this._mediaSettings = mediaSettings;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._cacheManager = cacheManager;
            this._categoryService = categoryService;
            this._localizationService = localizationService;
            this._urlRecordService = urlRecordService;
            this._catalogModelFactory = catalogModelFactory;
        }

        public IViewComponentResult Invoke(int? productThumbPictureSize, int categoryId)
        {
            var categoryIds = new List<int>();
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null)
                return Content("");

            var categoryModel = new CategoryModel
            {
                Id = category.Id,
                Name = _localizationService.GetLocalized(category, x => x.Name),
                Description = _localizationService.GetLocalized(category, x => x.Description),
                MetaKeywords = _localizationService.GetLocalized(category, x => x.MetaKeywords),
                MetaDescription = _localizationService.GetLocalized(category, x => x.MetaDescription),
                MetaTitle = _localizationService.GetLocalized(category, x => x.MetaTitle),
                SeName = _urlRecordService.GetSeName(category),
            };

            var pictureSize = _mediaSettings.CategoryThumbPictureSize;
            var childCategoryies = _categoryService.GetAllCategoriesByParentCategoryId(category.Id);

            foreach (var childcategory in childCategoryies)
            {
                var subCatModel = new CategoryModel.SubCategoryModel
                {
                    Id = childcategory.Id,
                    Name = _localizationService.GetLocalized(childcategory, y => y.Name),
                    SeName = _urlRecordService.GetSeName(childcategory),
                    Description = _localizationService.GetLocalized(childcategory, y => y.Description)
                };
                var grnadDhildCategoryies = _categoryService.GetAllCategoriesByParentCategoryId(subCatModel.Id);
                if(grnadDhildCategoryies != null && grnadDhildCategoryies.Count >0)
                {
                    foreach (var grandchildcategory in grnadDhildCategoryies)
                    {
                        var grandchildcategoryMolde = new CategoryModel.SubCategoryModel
                        {
                            Id = grandchildcategory.Id,
                            Name = _localizationService.GetLocalized(grandchildcategory, y => y.Name),
                            SeName = _urlRecordService.GetSeName(grandchildcategory),
                            Description = _localizationService.GetLocalized(grandchildcategory, y => y.Description)
                        };
                        subCatModel.ChildCategoryModels.Add(grandchildcategoryMolde);
                    }
                }
                categoryModel.SubCategories.Add(subCatModel);
            }
            
            var products = new List<Core.Domain.Catalog.Product>();
            var activeCategoryId = categoryId;
            if (categoryModel.SubCategories.FirstOrDefault() != null)
                activeCategoryId = categoryModel.SubCategories.FirstOrDefault().Id;

            var model = new HomePageProduct()
            {
                //Products = productOverviewModel,
                CurrentPage = 0,
                TotalPage = 0,
                CategoryProductBoxTemplate = _catalogModelFactory.PrepareCategoryProductBoxTemplateViewPath(category.CategoryProductBoxTemplateId),
                categoryModel = categoryModel,
                ActiveCategoryId = activeCategoryId
            };

            return View(model);
        }
    }
}