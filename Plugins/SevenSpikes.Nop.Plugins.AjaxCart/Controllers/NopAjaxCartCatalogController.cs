using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using SevenSpikes.Nop.Plugins.AjaxCart.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Controllers
{
    public class NopAjaxCartCatalogController : BasePublicController
    {
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IPictureService _pictureService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ILocalizationService _localizationService;
        private readonly NopAjaxCartSettings _nopAjaxCartSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public NopAjaxCartCatalogController(
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductAttributeService productAttributeService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            NopAjaxCartSettings nopAjaxCartSettings,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _productModelFactory = productModelFactory;
            _productService = productService;
            _productAttributeService = productAttributeService;
            _pictureService = pictureService;
            _nopAjaxCartSettings = nopAjaxCartSettings;
            _storeContext = storeContext;
            _workContext = workContext;
            _localizationService = localizationService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
        }

        public JsonResult CheckIfProductOrItsAssociatedProductsHasAttributes(int productId)
        {
            var flag = false;
            var productById = _productService.GetProductById(productId);
            if (!productById.IsRental)
            {
                if ((int) productById.ProductType == 5)
                    flag = CheckIfProductHasProductAttributes(productId, productById);
                else if ((int) productById.ProductType == 10)
                    flag = true;
            }
            else
                flag = true;

            return Json(new
            {
                HasProductAttributes = flag
            });
        }

        public ActionResult GetMiniProductDetailsView(int productId, bool isAddToCartButton = true,int packageid =0)
        {
            var productById = _productService.GetProductById(productId);
            if (productById == null || productById.Deleted || !productById.Published)
                return new ContentResult()
                {
                    Content = "Product is either deleted or not published"
                };
            var productDetailsModel = _productModelFactory.PrepareProductDetailsModel(productById, null, false);
            var str = _productModelFactory.PrepareProductTemplateViewPath(productById);
            //TODO://investigate
            /*if (NopAjaxCartCatalogController.\u003C\u003Eo__12.\u003C\u003Ep__0 == null)
            {
                NopAjaxCartCatalogController.\u003C\u003Eo__12.\u003C\u003Ep__0 =
                    CallSite<Func<CallSite, object, bool, object>>.Create(Binder.SetMember(CSharpBinderFlags.None,
                        "IsAddToCartButton", typeof(NopAjaxCartCatalogController),
                        (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
                        {
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null)
                        }));
            }*/

            productDetailsModel.PackageId = packageid;
            ViewBag.IsAddToCartButton = isAddToCartButton;
            return PartialView(str, productDetailsModel);
        }

        public ActionResult GetRelatedProducts(int productId)
        {
            var productList = _productService
                .GetProductsByIds(_productService.GetRelatedProductsByProductId1(productId, false)
                    .Take(_nopAjaxCartSettings.NumberOfRelatedProductsInPopup).ToList().Select(x => x.ProductId2)
                    .ToArray()).Where(productsById =>
                    _aclService.Authorize(productsById) && _storeMappingService.Authorize(productsById)).ToList();
            return PartialView("PopupRelatedProducts",
                _productModelFactory.PrepareProductOverviewModels(productList, true, true,
                    new int?(_nopAjaxCartSettings.ProductAddedToCartImageSize), false, false).ToList());
        }

        public ActionResult GetCrossSellProducts(int productId)
        {
            var productList = new List<Product>();
            var productsByProductId1 = _productService.GetCrossSellProductsByProductId1(productId, false);
            var shoppingCartProductIds = GetShoppingCartProductIds();
            var predicate = (Func<CrossSellProduct, bool>) (x => !shoppingCartProductIds.Contains(x.ProductId2));
            foreach (var productsById in _productService.GetProductsByIds(productsByProductId1.Where(predicate)
                .Select(x => x.ProductId2).ToArray()))
            {
                if (_aclService.Authorize(productsById) && _storeMappingService.Authorize(productsById))
                {
                    productList.Add(productsById);
                    if (productList.Count == _nopAjaxCartSettings.NumberOfCrossSellProductsInPopup)
                        break;
                }
            }

            return PartialView("PopupCrossSellProducts",
                _productModelFactory.PrepareProductOverviewModels(productList, true, true,
                    new int?(_nopAjaxCartSettings.ProductAddedToCartImageSize), false, false).ToList());
        }

        private bool CheckIfProductHasProductAttributes(int productId, Product product) => product.IsGiftCard ||
            product.CustomerEntersPrice ||
            !_nopAjaxCartSettings.EnableProductQuantityTextBox &&
            _productService.ParseAllowedQuantities(product).Length != 0 || _productAttributeService
                .GetProductAttributeMappingsByProductId(productId)
                .Any(attribute => (int) attribute.AttributeControlType != 50);

        private int[] GetShoppingCartProductIds() => _workContext.CurrentCustomer.ShoppingCartItems
            .Where(sci => (int) sci.ShoppingCartType == 1).LimitPerStore(_storeContext.CurrentStore.Id).ToList()
            .Select(x => x.ProductId).ToArray();
    }
}