using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Web.Factories;
using Nop.Web.Customization.Models.Catalog;
using Nop.Services.Localization;
using System.Web;
using static Nop.Web.Models.Catalog.CategoryModel;
using Nop.Core.Infrastructure;
using Nop.Core.Caching;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Customers;
using Nop.Services.Seo;
using Nop.Web.Models.Catalog;
using Nop.Web.Infrastructure.Cache;
using Nop.Services.Media;
using Nop.Web.Models.Media;
using Nop.Services.Helpers;

namespace Nop.Web.Controllers
{
    public partial class CustomCatalogController : BasePublicController
    {

        private readonly ICategoryService _categoryService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductTagService _productTagService;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly INopFileProvider _fileProvider;
        private readonly IStaticCacheManager _cacheManager;
        private readonly MediaSettings _mediaSettings;
        private readonly IWorkContext _workContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICustomeProductModelFactory _customeProductModelFactory;
        private readonly IUserAgentHelper _userAgentHelper;


        public CustomCatalogController(ICategoryService categoryService, ICatalogModelFactory catalogModelFactory,
            IProductModelFactory productModelFactory,
            IProductTagService productTagService,
            IProductService productService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            IStaticCacheManager cacheManager,
            MediaSettings mediaSettings,
            IWorkContext workContext,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            ICustomeProductModelFactory customeProductModelFactory,
            IUserAgentHelper userAgentHelper)
        {
            _priceFormatter = priceFormatter;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _pictureService = pictureService;
            this._categoryService = categoryService;
            this._catalogModelFactory = catalogModelFactory;
            this._productModelFactory = productModelFactory;
            this._productTagService = productTagService;
            this._productService = productService;
            this._webHelper = webHelper;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._fileProvider = fileProvider;
            _cacheManager = cacheManager;
            _customeProductModelFactory = customeProductModelFactory;
            _userAgentHelper = userAgentHelper;

        }

        #region Utilities
        protected BestSellerFilterModel PrepareBestSellerFilterHtml(int productTagId, IList<int> categoryIds, int categoryId, int subCategoryId, bool isDrawer = false)
        {
            var productTags = _productTagService.GetProductTagByCategoryId(categoryIds).ToList();

            var tagModel = new BestSellerFilterModel()
            {
                CategoryId = categoryId,
                SubCategoryId = subCategoryId,
                IsDrawer = isDrawer
            };

            tagModel.FilterTagModel.Add(new FilterTagModel()
            {
                ProductTagName = _localizationService.GetResource("Common.All"),
                ProductTagId = 0,
                IsSelected = productTagId == 0,
                URL = _webHelper.RemoveQueryString(_webHelper.GetUrlReferrer(), "ptid")
            });

            foreach (var productTag in productTags)
            {
                tagModel.FilterTagModel.Add(new FilterTagModel()
                {
                    ProductTagName = productTag.Name,
                    ProductTagId = productTag.Id,
                    IsSelected = productTagId == productTag.Id,
                    URL = _webHelper.ModifyQueryString(_webHelper.GetUrlReferrer(), "ptid", productTag.Id.ToString())
                });
            }

            return tagModel;
        }

        #endregion

        public virtual IActionResult CatalogProducts(int categoryId, bool loadSubCategoryProduct = true, bool isProduct = false)
        {
            var isOwlLoopTrue = true;

            var mapHtml = "";
            int isDisplayMap = 2; // 0 for hide map area
            if (isProduct)
                isDisplayMap = 0; // 0 for do not effact map area

            //, string priceRange = ""
            var url = _webHelper.GetUrlReferrer();
            Uri myUri = new Uri(_webHelper.GetUrlReferrer());
            string ptid = HttpUtility.ParseQueryString(myUri.Query).Get("ptid");
            int.TryParse(ptid, out int productTagId);
            var categoryIds = new List<int>();
            var subCategoryModels = new List<SubCategoryModel>();

            if (loadSubCategoryProduct)
            {
                var categories = _categoryService.GetAllCategoriesByParentCategoryId(categoryId);
                if (categories != null && categories.Count() > 0)
                {
                    for (int i = 0; i < categories.Count; i++)
                    {
                        var item = categories[i];
                        if (i == 0)
                        {
                            categoryIds.Add(item.Id);
                        }
                        var data = new SubCategoryModel()
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Description = item.Description
                        };
                        subCategoryModels.Add(data);
                    }
                }
                else
                {
                    categoryIds.Add(categoryId);
                }
            }
            else
            {
                categoryIds.Add(categoryId);
            }
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null)
                return Json(new { html = "", mapHtml = mapHtml, displayMap = isDisplayMap });

            var CategoryProductBoxTemplate = _catalogModelFactory.PrepareCategoryProductBoxTemplateViewPath(category.CategoryProductBoxTemplateId);
            if (CategoryProductBoxTemplate == null)
                return Json(new { html = "", mapHtml = mapHtml, displayMap = isDisplayMap });

            var tagModel = PrepareBestSellerFilterHtml(
                    productTagId: productTagId,
                    categoryIds: categoryIds,
                    categoryId: categoryId,
                    subCategoryId: 0);

            //load products
            var products = _productService.SearchProducts(out IList<int> filterableSpecificationAttributeOptionIds,
                true,
                categoryIds: categoryIds,
                productTagId: productTagId,
                storeId: _storeContext.CurrentStore.Id);

            if (!_userAgentHelper.IsMobileDevice())
            {
                if (products.Count <= 3)
                    isOwlLoopTrue = false;
            }
            if (_userAgentHelper.IsMobileDevice())
            {
                if (products.Count == 1)
                    isOwlLoopTrue = false;
            }
            if (!products.Any())
                return Json(new { html = "", mapHtml = mapHtml, displayMap = isDisplayMap, isOwlLoopTrue = isOwlLoopTrue });

            var productOverviewModels = _productModelFactory.PrepareProductOverviewModels(products, true, true).ToList();
            var model = new CategoryProductModel()
            {
                CategoryId = categoryId,
                SubCategories = subCategoryModels,
                CategoryProductBoxTemplate = CategoryProductBoxTemplate,
                ProductOverviewModel = productOverviewModels
            };

            var viewName = _catalogModelFactory.PrepareCategoryTemplateViewPath(category.CategoryTemplateId);
            string commonPath = "~/Themes/Eshop2021/Views/Catalog/_ProductsInGridOrLines_CatalogProduct.cshtml";
            var relativePath = $"~/Themes/Eshop2021/Views/Catalog/{viewName}.PartialTab.cshtml";
            var absolutePath = _fileProvider.MapPath(relativePath);
            if (System.IO.File.Exists(absolutePath))
            {
                commonPath = relativePath;
            }
       
            if (isDisplayMap == 2)
            {
                var relativeMapPath = $"~/Themes/Eshop2021/Views/Catalog/{viewName}.PartialMap.cshtml";
                var absoluteMapPath = _fileProvider.MapPath(relativeMapPath);
                if (System.IO.File.Exists(absoluteMapPath))
                {
                    isDisplayMap = 1;
                    mapHtml = RenderPartialViewToString($"~/Themes/Eshop2021/Views/Catalog/{viewName}.PartialMap.cshtml",  category.Name);
                }
            }

            return Json(new
            {
                html = RenderPartialViewToString(commonPath, model),
                filterhtml = RenderPartialViewToString("_BestSellerForCatalogPage", tagModel),
                displayMap = isDisplayMap,
                mapHtml = mapHtml,
                isOwlLoopTrue = isOwlLoopTrue
            });
        }

        public virtual IActionResult GetCategoriesBySubCategories(int categoryId, int subCategoryId, CatalogPagingFilteringModel command, int productTagId = 0)
        {
            var isOwlLoopTrue = true;

            var category = _categoryService.GetCategoryById(subCategoryId);
            var subCategoryProductBoxTemplate = "";
            if (category == null || category.Deleted)
                return Content("");
            if (category != null)
            {
                subCategoryProductBoxTemplate = _catalogModelFactory.PrepareCategoryProductBoxTemplateViewPath(category.CategoryProductBoxTemplateId);
            }
            if (subCategoryProductBoxTemplate == "_ProductBoxMobilePlan")
            {
                var model = _catalogModelFactory.PrepareCategoryModel(category, command);
                model.ActiveCategoryId = subCategoryId;
                model.OwlCarouselDivId = "";

                model.OwlCarouselDivId = $"home-page-product-{categoryId}-{subCategoryId}";

                if (!_userAgentHelper.IsMobileDevice())
                {
                    if (model.SubCategories.Count <= 3)
                        isOwlLoopTrue = false;
                }
                if (_userAgentHelper.IsMobileDevice())
                {
                    if (model.SubCategories.Count == 1)
                        isOwlLoopTrue = false;
                }
                return Json(new
                {
                    html = RenderPartialViewToString("~/Themes/Eshop2021/Views/Shared/Components/CustomShowWithSubCategory/_Category_SubCategory.cshtml", model),
                    filterhtml = "",
                    isOwlLoopTrue = isOwlLoopTrue
                });
            }
            else
            {
                var categoryIds = new List<int>();
                categoryIds.Add(subCategoryId);

                var products = _productService.SearchProducts(out IList<int> filterableSpecificationAttributeOptionIds,
                    true,
                    categoryIds: categoryIds,
                    productTagId: productTagId,
                    storeId: _storeContext.CurrentStore.Id,
                    visibleIndividuallyOnly: true);

                if (!products.Any())
                    return Json(new { html = "", filterhtml = "" });

                var productOverviewModel = new List<ProductOverviewModel>();
                if (productTagId == 0)
                {
                    var productList = products.Where(p => p.ShowOnHomePage).ToList();
                    productOverviewModel = _productModelFactory.PrepareProductOverviewModels(productList, true, true).ToList();
                }
                else
                {
                    productOverviewModel = _productModelFactory.PrepareProductOverviewModels(products, true, true).ToList();
                }
                foreach (var item in productOverviewModel)
                {
                    var product = products.Where(p => p.Id == item.Id).FirstOrDefault();
                    if (product.ProductAttributeMappings.Count > 0)
                    {
                        item.ProductAttributeOverviewModels = _customeProductModelFactory.PrepareProductAttributeModels(product);
                    }

                    item.IsOutOfStock = _productService.GetTotalStockQuantity(product) <= 0;
                }
                var model = new HomePageProduct()
                {
                    Products = productOverviewModel,
                    CategoryProductBoxTemplate = _catalogModelFactory.PrepareCategoryProductBoxTemplateViewPath(category.CategoryProductBoxTemplateId),
                    ActiveCategoryId = subCategoryId
                };
                model.OwlCarouselDivId = $"home-page-product-{categoryId}-{subCategoryId}";
                var tagModel = PrepareBestSellerFilterHtml(productTagId, categoryIds, categoryId, subCategoryId, false);

                if (!_userAgentHelper.IsMobileDevice())
                {
                    if (model.Products.Count <= 3)
                        isOwlLoopTrue = false;
                }
                if (_userAgentHelper.IsMobileDevice())
                {
                    if (products.Count == 1)
                        isOwlLoopTrue = false;
                }
                return Json(new
                {
                    html = RenderPartialViewToString("~/Themes/Eshop2021/Views/Shared/Components/HomepageProducts/_Category" + model.CategoryProductBoxTemplate + ".cshtml", model),
                    filterhtml = RenderPartialViewToString("_BestSellerForHomePage", tagModel)
                });
            }
        }

        public virtual IActionResult LoadMobilePlanSubCategory(int categoryId, int subCategoryId, CatalogPagingFilteringModel command, int productTagId = 0)
        {   
            var category = _categoryService.GetCategoryById(subCategoryId);
            if (category == null || category.Deleted)
                return Content("");
            var subCategoryProductBoxTemplate = "";
            if (category != null)
            {
                subCategoryProductBoxTemplate = _catalogModelFactory.PrepareCategoryProductBoxTemplateViewPath(category.CategoryProductBoxTemplateId);
            }
            if (subCategoryProductBoxTemplate == "_ProductBoxMobilePlan")
            {   
                var model = _catalogModelFactory.PrepareCategoryModel(category, command);
                model.ActiveCategoryId = subCategoryId;
                model.OwlCarouselDivId = "";

                return Json(new
                {
                    html = RenderPartialViewToString("~/Themes/Eshop2021/Views/Catalog/_ProductsInGridOrLines_MobilePlan.cshtml", model),
                    filterhtml = ""
                });
            }
            else
            {
                var categoryIds = new List<int>();
                categoryIds.Add(subCategoryId);

                var products = _productService.SearchProducts(out IList<int> filterableSpecificationAttributeOptionIds,
                    true,
                    categoryIds: categoryIds,
                    productTagId: productTagId,
                    storeId: _storeContext.CurrentStore.Id,
                    visibleIndividuallyOnly: true);

                if (!products.Any())
                    return Json(new { html = "", filterhtml = "" });

                var productOverviewModel = new List<ProductOverviewModel>();
                productOverviewModel = _productModelFactory.PrepareProductOverviewModels(products, true, true).ToList();
                foreach (var item in productOverviewModel)
                {
                    var product = products.Where(p => p.Id == item.Id).FirstOrDefault();
                    if (product.ProductAttributeMappings.Count > 0)
                    {
                        item.ProductAttributeOverviewModels = _customeProductModelFactory.PrepareProductAttributeModels(product);
                    }

                    item.IsOutOfStock = _productService.GetTotalStockQuantity(product) <= 0;
                }
                var model = new CategoryProductModel()
                {
                    CategoryId = subCategoryId,
                    CategoryProductBoxTemplate = subCategoryProductBoxTemplate,
                    ProductOverviewModel = productOverviewModel
                };
                
                var tagModel = PrepareBestSellerFilterHtml(productTagId, categoryIds, categoryId, subCategoryId, false);
                return Json(new
                {
                    html = RenderPartialViewToString("~/Themes/Eshop2021/Views/Catalog/_ProductsInGridOrLines_CatalogProduct.cshtml", model),
                    filterhtml = RenderPartialViewToString("_BestSellerForCatalogPageProduct", tagModel)
                });
            }
        }
    }
}
