using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Web.Customization.Models.Catalog;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Logging;
using Nop.Services.Common;
using Nop.Services.Stores;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Services.Helpers;

namespace Nop.Web.Controllers
{
    public partial class CustomProductController : BasePublicController
    {
        #region Fields
        private readonly ICategoryService _categoryService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly ICustomeProductModelFactory _customeProductModelFactory;
        private readonly IProductTagService _productTagService;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IWebHelper _webHelper;
        private readonly CatalogSettings _catalogSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IAclService _aclService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly IUserAgentHelper _userAgentHelper;
        #endregion

        #region Ctor
        public CustomProductController(ICategoryService categoryService,
            ICatalogModelFactory catalogModelFactory,
            ICustomeProductModelFactory customeProductModelFactory,
            IProductTagService productTagService,
            IProductService productService,
            IStoreContext storeContext,
            IProductModelFactory productModelFactory,
            IWebHelper webHelper,
            CatalogSettings catalogSettings,
            ILocalizationService localizationService,
            IAclService aclService,

            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            IPermissionService permissionService,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            IUserAgentHelper userAgentHelper)
        {
            _categoryService = categoryService;
            _catalogModelFactory = catalogModelFactory;
            _customeProductModelFactory = customeProductModelFactory;
            _productTagService = productTagService;
            _productService = productService;
            _storeContext = storeContext;
            _productModelFactory = productModelFactory;
            _webHelper = webHelper;
            _catalogSettings = catalogSettings;
            _localizationService = localizationService;
            _aclService = aclService;


            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _permissionService = permissionService;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _userAgentHelper = userAgentHelper;
        }
        #endregion

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

        #region Methods
        public virtual IActionResult GetCategoryProducts(int categoryId, int productTagId = 0, int parentCategoryId = 0)
        {
            var isOwlLoopTrue = true;

            var model = new HomePageProduct();
            var categoryIds = new List<int>();
            var category = _categoryService.GetCategoryById(categoryId);
            var parentCategory = _categoryService.GetCategoryById(parentCategoryId);
            if (category == null)
            {
                return Json(new
                {
                    html = RenderPartialViewToString("~/Themes/Eshop2021/Views/Shared/Components/CustomShowWithSubCategory/_CategoryDrawerProduct.cshtml", model),
                    filterhtml = ""
                });
            }
            categoryIds.Add(categoryId);

            var products = _productService.SearchProducts(out IList<int> filterableSpecificationAttributeOptionIds,
                true,
                pageSize: 10,
                categoryIds: categoryIds,
                productTagId: productTagId,
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true);

            if (!_userAgentHelper.IsMobileDevice())
            {
                if (products.Count <= 3)
                    isOwlLoopTrue = false;
            }
            //if (!products.Any())
            //    return Json(new { html = "", filterhtml = "" });

            var productOverviewModel = new List<ProductOverviewModel>();
            productOverviewModel = _productModelFactory.PrepareProductOverviewModels(products, true, true).ToList();

            foreach (var item in productOverviewModel)
            {
                var product = products.FirstOrDefault(p => p.Id == item.Id);

                var productColotAttrib = product.ProductAttributeMappings.Where(p => p.AttributeControlType == AttributeControlType.ColorSquares);
                if (productColotAttrib.Any())
                {
                    item.ProductAttributeOverviewModels = _customeProductModelFactory.PrepareProductAttributeModels(product);
                }
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    item.IsOutOfStock = _productService.GetTotalStockQuantity(product) <= 0;
            }
            var cat = new CategoryModel()
            {
                Id = category.Id,
                Name = _localizationService.GetLocalized(category, x => x.Name),
                Description = _localizationService.GetLocalized(category, x => x.Description)
            };

            model.Products = productOverviewModel;
            model.ParentCategoryName = _localizationService.GetLocalized(parentCategory, x => x.Name);
            model.CategoryProductBoxTemplate = _catalogModelFactory.PrepareCategoryProductBoxTemplateViewPath(category.CategoryProductBoxTemplateId);
            model.categoryModel = cat;
            model.OwlCarouselDivId = $"drawer-prd-slider-{categoryId}";
            model.IsDrawer = true;

            var tagModel = PrepareBestSellerFilterHtml(productTagId, categoryIds, categoryId, 0, true);
            return Json(new
            {
                html = RenderPartialViewToString("~/Themes/Eshop2021/Views/Shared/Components/CustomShowWithSubCategory/_CategoryDrawerProduct.cshtml", model),
                filterhtml = RenderPartialViewToString("_BestSellerForHomePage", tagModel),
                isOwlLoopTrue = isOwlLoopTrue
            });
        }

        #region HomePage
        public virtual IActionResult GetHomePageProduct(int categoryId, int subCategoryId, CatalogPagingFilteringModel command, int productTagId = 0, bool isDrawer = false)
        {
            var isOwlLoopTrue = true;

            var subCategory = _categoryService.GetCategoryById(subCategoryId);
            var categoryIds = new List<int>();
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null)
                return Json(new { html = "", filterhtml = "", isOwlLoopTrue=isOwlLoopTrue });

            if (isDrawer)
            {
                categoryIds.Add(categoryId);
                categoryIds.AddRange(_categoryService.GetChildCategoryIds(categoryId, _storeContext.CurrentStore.Id));
            }
            else
                categoryIds.Add(subCategoryId);

            //var products = _productService.SearchProducts(out IList<int> filterableSpecificationAttributeOptionIds,
            //    true,
            //    pageSize: 10,
            //    categoryIds: categoryIds,
            //    productTagId: productTagId,
            //    storeId: _storeContext.CurrentStore.Id,
            //    visibleIndividuallyOnly: true);

            var showonHomePage = false;
            if (!isDrawer && productTagId == 0)
                showonHomePage = true;

            var products = _productService.GetProductsByCategoryFilter(categoryIds, showonHomePage, productTagId, 0, 8);

            if (!products.Any())
                return Json(new { html = "", filterhtml = "", isOwlLoopTrue = isOwlLoopTrue });

            var productOverviewModel = _productModelFactory.PrepareProductOverviewModels(products, true, true).ToList();
            
            foreach (var item in productOverviewModel)
            {
                var product = products.Where(p => p.Id == item.Id).FirstOrDefault();
                var productColotAttrib = product.ProductAttributeMappings.Where(p => p.AttributeControlType == AttributeControlType.ColorSquares);
                if (productColotAttrib.Any())
                {
                    item.ProductAttributeOverviewModels = _customeProductModelFactory.PrepareProductAttributeModels(product);
                }

                if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
                    item.IsOutOfStock = _productService.GetTotalStockQuantity(product) <= 0;
            }

            var productTemplate = _catalogModelFactory.PrepareCategoryProductBoxTemplateViewPath(category.CategoryProductBoxTemplateId);
            if (productTemplate == "_ProductBoxMobilePlan")
            {
                productTemplate = _catalogModelFactory.PrepareCategoryProductBoxTemplateViewPath(subCategory.CategoryProductBoxTemplateId);
            }

            var model = new HomePageProduct()
            {
                Products = productOverviewModel,
                CategoryProductBoxTemplate = productTemplate,
                ActiveCategoryId = subCategoryId,
                IsDrawer = isDrawer
            };

            if (isDrawer)
                model.OwlCarouselDivId = $"drawer-prd-slider-{categoryId}";
            else
                model.OwlCarouselDivId = $"home-page-product-{categoryId}-{subCategoryId}";

            var tagModel = PrepareBestSellerFilterHtml(productTagId, categoryIds, categoryId, subCategoryId, isDrawer);

            if (!_userAgentHelper.IsMobileDevice())
            {
                if (model.Products.Count <= 3)
                    isOwlLoopTrue = false;
            }
            return Json(new
            {
                html = RenderPartialViewToString("~/Themes/Eshop2021/Views/Shared/Components/HomepageProducts/_Category" + model.CategoryProductBoxTemplate + ".cshtml", model),
                filterhtml = RenderPartialViewToString("_BestSellerForHomePage", tagModel),
                isOwlLoopTrue = isOwlLoopTrue
            });
        }
        #endregion

        #region Device Catalog
        public virtual IActionResult CatalogFilterProduct(int categoryId, bool loadmore = false)
        {
            var isOwlLoopTrue = true;
            //, string priceRange = ""
            var url = _webHelper.GetUrlReferrer();
            int pageIndex = 0;
            int pageSize = 0;
            int productTagId = 0;
            int manufacturerId = 0;
            int offer = 0;
            string specs = string.Empty;
            string priceRange = string.Empty;
            var q = "";

            if (!string.IsNullOrEmpty(url))
            {
                Uri myUri = new Uri(url);
                string ptid = HttpUtility.ParseQueryString(myUri.Query).Get("ptid");
                string mId = HttpUtility.ParseQueryString(myUri.Query).Get("mId");
                string pi = HttpUtility.ParseQueryString(myUri.Query).Get("pi");
                string ps = HttpUtility.ParseQueryString(myUri.Query).Get("ps");
                specs = HttpUtility.ParseQueryString(myUri.Query).Get("specs");
                priceRange = HttpUtility.ParseQueryString(myUri.Query).Get("prange");
                string ofr = HttpUtility.ParseQueryString(myUri.Query).Get("ofr");
                q = HttpUtility.ParseQueryString(myUri.Query).Get("q");

                int.TryParse(pi, out pageIndex);
                int.TryParse(ps, out pageSize);
                int.TryParse(ptid, out productTagId);
                int.TryParse(mId, out manufacturerId);
                int.TryParse(ofr, out offer);
            }
            

            var filteredsa = new List<int>();
            if (!string.IsNullOrEmpty(specs))
                filteredsa = specs.Split(',').Select(int.Parse).ToList();

            if (pageSize == 0)
                pageSize = _catalogSettings.DefaultCategoryPageSize;

            pageIndex = pageIndex != 0 ? pageIndex - 1 : pageIndex;

            decimal? priceMin = null, priceMax = null;
            if (!string.IsNullOrEmpty(priceRange))
            {
                var prices = priceRange.Split(",");
                if (prices.Length > 0)
                {
                    if (decimal.TryParse(prices[0], out decimal tmppriceMin))
                        priceMin = tmppriceMin;
                }

                if (prices.Length > 1)
                {
                    if (decimal.TryParse(prices[1], out decimal tmppriceMax))
                        priceMax = tmppriceMax;
                }
            }

            var categoryIds = new List<int>();
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null)
                return Json(new { html = "", filterhtml = "", hideLoadMore = true, maxPrice = 0 });

            categoryIds.Add(categoryId);
            categoryIds.AddRange(_categoryService.GetChildCategoryIds(categoryId, _storeContext.CurrentStore.Id));

            var tagModel = PrepareBestSellerFilterHtml(
                    productTagId: productTagId,
                    categoryIds: categoryIds,
                    categoryId: categoryId,
                    subCategoryId: 0);

            var specfilter = _productService.SearchProducts(out IList<int> allfilterableSpecificationAttributeOptionIds,
                true,
                categoryIds: categoryIds,
                pageIndex: 0,
                pageSize: 1);

            var products = _productService.SearchProducts(out IList<int> filterableSpecificationAttributeOptionIds,
                true,
                pageIndex: pageIndex,
                pageSize: pageSize,
                categoryIds: categoryIds,
                manufacturerId: manufacturerId,
                productTagId: productTagId,
                keywords: q,
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                priceMin: priceMin,
                priceMax: priceMax,
                filteredSpecs: filteredsa,
                markedAsNewOnly: offer == 1);

            if (!_userAgentHelper.IsMobileDevice())
            {
                if (products.Count <= 3)
                    isOwlLoopTrue = false;
            }
            var specFilterModel = _catalogModelFactory.PrepareCustomSpecificationAttributeFilter(allfilterableSpecificationAttributeOptionIds.ToArray(), filteredsa, filterableSpecificationAttributeOptionIds.ToArray());
            if (!products.Any())
            {
                return Json(new
                {
                    html = "",
                    filterhtml = RenderPartialViewToString("_BestSellerForCatalogPage", tagModel),
                    specFilter = RenderPartialViewToString("_ProductSpecificationAttribureFilter", specFilterModel),
                    hideLoadMore = true,
                    maxPrice = _productService.GetMaxPriceByCategoryId(categoryId, manufacturerId),
                    isOwlLoopTrue = isOwlLoopTrue
                });
            }

            var productOverviewModel = _productModelFactory.PrepareProductOverviewModels(products, true, true).ToList();

            foreach (var item in productOverviewModel)
            {
                var product = products.Where(p => p.Id == item.Id).FirstOrDefault();
                var productColotAttrib = product.ProductAttributeMappings.Where(p => p.AttributeControlType == AttributeControlType.ColorSquares);
                if (productColotAttrib.Any())
                {
                    item.ProductAttributeOverviewModels = _customeProductModelFactory.PrepareProductAttributeModels(product);
                }

                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    item.IsOutOfStock = _productService.GetTotalStockQuantity(product) <= 0;
            }
            bool hideLoadMore = products.TotalPages - 1 == pageIndex;

            return Json(new
            {
                html = RenderPartialViewToString("~/Themes/Eshop2021/Views/Catalog/_ProductsInGridOrLines_Product.cshtml", productOverviewModel),
                filterhtml = RenderPartialViewToString("_BestSellerForCatalogPage", tagModel),
                specFilter = RenderPartialViewToString("_ProductSpecificationAttribureFilter", specFilterModel),
                hideLoadMore = hideLoadMore,
                maxPrice = _productService.GetMaxPriceByCategoryId(categoryId, manufacturerId),
                isOwlLoopTrue = isOwlLoopTrue
            });
        }
        #endregion

        #region ProductCompanre
        public virtual IActionResult AddProductToCompare(int categoryId, CatalogPagingFilteringModel command)
        {
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted)
                return InvokeHttp404();

            var notAvailable =
                //published?
                !category.Published ||
                //ACL (access control list) 
                !_aclService.Authorize(category) ||
                //Store mapping
                !_storeMappingService.Authorize(category);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            if (notAvailable && !_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);


            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewCategory",
                string.Format(_localizationService.GetResource("ActivityLog.PublicStore.ViewCategory"), category.Name), category);

            //model
            var model = _catalogModelFactory.PrepareCategoryModel(category, command);

            //template
            //var templateViewPath = _catalogModelFactory.PrepareCategoryTemplateViewPath(category.CategoryTemplateId);
            //return View(templateViewPath, model);
            return Json(new { html = RenderPartialViewToString($"~/Themes/Eshop2021/Views/Catalog/AddCompareProductPopup/CategoryTemplate.ProductsInGridOrLines.cshtml", model) });
        }
        #endregion

        #region Service Products
        public virtual IActionResult ServiceProducts()
        {
            var products = _productService.GetServiceProducts();

            var productOverviewModel = new List<ProductOverviewModel>();
            productOverviewModel = _productModelFactory.PrepareProductOverviewModels(products, true, true).ToList();

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            foreach (var item in productOverviewModel)
            {
                var cartProduct = cart.Where(x => x.Product.Id == item.Id).FirstOrDefault();
                if(cartProduct != null)
                {
                    item.IsServiceProductAddedToCart = true;
                }
            }

            return Json(new
            {
                html = RenderPartialViewToString("~/Themes/Eshop2021/Views/Shared/_ServiceProducts.cshtml", productOverviewModel),
            });
        }
        #endregion

        #endregion
    }
}
