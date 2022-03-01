using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Enum;
using Nop.Services.Catalog;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using System.Linq;

namespace Nop.Web.Components
{
    public class ProductPlanViewComponent : NopViewComponent
    {
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ICustomeProductModelFactory _customeProductModelFactory;

        public ProductPlanViewComponent(IProductService productService, 
            IProductModelFactory productModelFactory,
            ICustomeProductModelFactory customeProductModelFactory)
        {
            this._productService = productService;
            this._productModelFactory = productModelFactory;
            _customeProductModelFactory = customeProductModelFactory;
        }

        public IViewComponentResult Invoke(int? productId, int? productThumbPictureSize, EnumProductDetail enumProductDetail)
        {
            var product = _productService.GetProductById((productId ?? 0));
            if (product.RequireAnyOneFromOtherProducts && !string.IsNullOrEmpty(product.RequiredAnyOneFromOtherProductIds))
            {
                var requiredProductIds = product.RequiredAnyOneFromOtherProductIds.Split(',').Select(int.Parse).ToArray();
                var products = _productService.GetProductsByIds(requiredProductIds);
                var productOverviewModels = _productModelFactory.PrepareProductOverviewModels(products, true, true, productThumbPictureSize, true).ToList();
                foreach (var item in productOverviewModels)
                {
                    var prd = products.Where(p => p.Id == item.Id).FirstOrDefault();
                    if (prd.ProductAttributeMappings.Count > 0)
                    {
                        item.ProductAttributeOverviewModels = _customeProductModelFactory.PrepareProductAttributeModels(prd);
                    }

                    item.IsOutOfStock = _productService.GetTotalStockQuantity(prd) <= 0;
                }
                var model = new BoughtTogether()
                {
                    ProductOverviewModel = productOverviewModels,
                    ProductId = (productId ?? 0),
                    enumProductDetail = enumProductDetail
                };
                return View(model);
            }

            return Content("");
        }
    }
}