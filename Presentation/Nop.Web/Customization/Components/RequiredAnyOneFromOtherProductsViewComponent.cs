using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using System.Linq;

namespace Nop.Web.Customization.Components
{
    public class RequiredAnyOneFromOtherProductsViewComponent : NopViewComponent
    {
        #region Fields
        public const string PRODUCTS_REQUIRED_IDS_KEY = "Nop.pres.related.required-{0}-{1}";
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        public RequiredAnyOneFromOtherProductsViewComponent(IStoreContext storeContext, 
            IAclService aclService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IStoreMappingService storeMappingService)
        {
            this._storeContext = storeContext;
            this._aclService = aclService;
            this._productModelFactory = productModelFactory;
            this._productService = productService;
            this._storeMappingService = storeMappingService;
        }

        public IViewComponentResult Invoke(int productId, int? productThumbPictureSize)
        {
            var productIds = EngineContext.Current.Resolve<IStaticCacheManager>().Get(
                string.Format(PRODUCTS_REQUIRED_IDS_KEY, productId, _storeContext.CurrentStore.Id),
                () => _productService.ParseRequiredAnyOneFromOtherProductIds(_productService.GetProductById(productId)));

            //load products
            var products = _productService.GetProductsByIds(productIds);
            //load products
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();
            //visible individually
            products = products.Where(p => p.VisibleIndividually).ToList();

            if (!products.Any())
                return Content("");

            var model = _productModelFactory.PrepareProductOverviewModels(products, true, true, productThumbPictureSize)
                .ToList();
            return View(model);
        }
    }
}
