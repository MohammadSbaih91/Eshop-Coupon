using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using System.Collections.Generic;
using Nop.Web.Customization.Models.Catalog;

namespace Nop.Web.Components
{
    public class CategoryProductsViewComponent : NopViewComponent
    {
        private readonly ICategoryService _categoryService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;

        public CategoryProductsViewComponent(ICategoryService categoryService, ICatalogModelFactory catalogModelFactory,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IStoreContext storeContext)
        {
            this._categoryService = categoryService;
            this._catalogModelFactory = catalogModelFactory;
            this._productModelFactory = productModelFactory;
            this._productService = productService;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke(int categoryId)
        {
            var category = _categoryService.GetCategoryById(categoryId);
            var CategoryProductBoxTemplate = _catalogModelFactory.PrepareCategoryProductBoxTemplateViewPath(category.CategoryProductBoxTemplateId);

            //load products
            var products = _productService.SearchProducts(
                       categoryIds: new List<int> { categoryId },
                       storeId: _storeContext.CurrentStore.Id);
            
            if (!products.Any())
                return Content("");

            var productOverviewModels = _productModelFactory.PrepareProductOverviewModels(products, true, true).ToList();
            var model = new CategoryProductModel()
            {
                CategoryId = categoryId,
                CategoryProductBoxTemplate = CategoryProductBoxTemplate,
                ProductOverviewModel = productOverviewModels
            };
            return View(model);
        }
    }
}