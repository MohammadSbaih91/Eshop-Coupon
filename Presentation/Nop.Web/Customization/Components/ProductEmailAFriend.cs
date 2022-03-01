using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Customization.Components
{
    public class ProductEmailAFriendViewComponent : NopViewComponent
    {
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly CatalogSettings _catalogSettings;

        public ProductEmailAFriendViewComponent(IProductService productService, IProductModelFactory productModelFactory, CatalogSettings catalogSettings)
        {
            _productService = productService;
            _productModelFactory = productModelFactory;
            _catalogSettings = catalogSettings;
        }

        public IViewComponentResult Invoke(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return Content(string.Empty);

            var model = new ProductEmailAFriendModel();
            model = _productModelFactory.PrepareProductEmailAFriendModel(model, product, false);
            return View(model);
        }
    }
}
