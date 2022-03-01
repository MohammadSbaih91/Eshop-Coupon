using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.ShoppingCart;
using SevenSpikes.Nop.Plugins.AjaxCart.Domain;
using SevenSpikes.Nop.Plugins.AjaxCart.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Controllers
{
    public class NopAjaxCartShoppingCartController : ShoppingCartController
    {
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IUrlRecordService _urlRecordService;
        private readonly NopAjaxCartSettings _ajaxCartSettings;
        private readonly IProductAttributeFormatter _productAttributeFormatter;

        public NopAjaxCartShoppingCartController(
            NopAjaxCartSettings ajaxCartSettings,
            IProductAttributeFormatter productAttributeFormatter,
            CaptchaSettings captchaSettings,
            CustomerSettings customerSettings,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IDownloadService downloadService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            ShoppingCartSettings shoppingCartSettings)
            : base(captchaSettings, customerSettings, checkoutAttributeParser, checkoutAttributeService,
                currencyService, customerActivityService, customerService, discountService, downloadService,
                genericAttributeService, giftCardService, localizationService, fileProvider, permissionService,
                pictureService, priceCalculationService, priceFormatter, productAttributeParser,
                productAttributeService, productService, shoppingCartModelFactory, shoppingCartService, cacheManager,
                storeContext, taxService, urlRecordService, webHelper, workContext, workflowMessageService,
                mediaSettings, orderSettings, shoppingCartSettings)
        {
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _taxService = taxService;
            _currencyService = currencyService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _permissionService = permissionService;
            _workContext = workContext;
            _shoppingCartSettings = shoppingCartSettings;
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _urlRecordService = urlRecordService;
            _ajaxCartSettings = ajaxCartSettings;
            _productAttributeFormatter = productAttributeFormatter;
        }

        public JsonResult AddProductToCartAjax(
            int productId,
            int quantity,
            bool isAddToCartButton,
            int packageId=0)
        {
            return AddProductToCartAjaxInternal(productId, quantity, isAddToCartButton, packageId);
        }

        public JsonResult AddProductFromProductDetailsPageToCartAjax(
            int productId,
            bool isAddToCartButton,
            IFormCollection form)
        {
            return AddProductFromProductDetailsPageToCartAjaxInternal(productId, isAddToCartButton, form);
        }

        public ActionResult MiniShoppingCart()
        {
            if (!_shoppingCartSettings.MiniShoppingCartEnabled)
                return Content("");
            return !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart)
                ? (ActionResult) Content("")
                : PartialView(_shoppingCartModelFactory.PrepareMiniShoppingCartModel());
        }

        public ActionResult NopAjaxCartFlyoutShoppingCart() => ViewComponent("FlyoutShoppingCart");

        private MiniShoppingCartModel GetShoppingCartItemModel(int productId,int quantity)
        {
            var shoppingCartModel = _shoppingCartModelFactory.PrepareMiniShoppingCartModel();
            var product = _productService.GetProductById(productId);
            var shoppingCartItemModel1 = new MiniShoppingCartModel.ShoppingCartItemModel
            {
                ProductId = productId,
                ProductName = _localizationService.GetLocalized(product, x => x.Name),
                ProductSeName = _urlRecordService.GetSeName(product),
                Quantity = quantity
            };
          
            var shoppingCartItemModel2 = shoppingCartItemModel1;
            var shoppingCartItem = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => (int) sci.ShoppingCartType == 1).ToList().Where(i => i.Product == product)
                .OrderByDescending(i => i.UpdatedOnUtc).FirstOrDefault();
            if (shoppingCartItem == null)
                return new MiniShoppingCartModel();

            var num2 = _currencyService.ConvertFromPrimaryStoreCurrency(
                _taxService.GetProductPrice(product, _priceCalculationService.GetUnitPrice(shoppingCartItem, true),
                    out var num1), _workContext.WorkingCurrency);
            shoppingCartItemModel2.UnitPrice = _priceFormatter.FormatPrice(num2);
            shoppingCartItemModel2.Picture = _shoppingCartModelFactory.PrepareCartItemPictureModel(shoppingCartItem,
                _ajaxCartSettings.ProductAddedToCartImageSize, true, shoppingCartItemModel2.ProductName);
            shoppingCartItemModel2.AttributeInfo =
                _productAttributeFormatter.FormatAttributes(shoppingCartItem.Product, shoppingCartItem.AttributesXml);
            shoppingCartModel.Items.Clear();
            shoppingCartModel.Items.Add(shoppingCartItemModel2);
            return shoppingCartModel;
        }

        private JsonResult AddProductToCartAjaxInternal(
            int productId,
            int quantity,
            bool isAddToCartButton,
            int packageId)
        {
            var toCartResultModel = new AddProductToCartResultModel()
            {
                Status = "success",
                AddToCartWarnings = string.Empty,
                ErrorMessage = string.Empty,
                RedirectUrl = string.Empty
            };
            var productById = _productService.GetProductById(productId);
            if (productById == null)
            {
                toCartResultModel.Status = "error";
                toCartResultModel.PopupTitle =
                    _localizationService.GetResource("SevenSpikes.NopAjaxCart.PopupTitle.Error");
                toCartResultModel.ErrorMessage = $"Product with Id {productId} cannot be found";
                return Json(toCartResultModel);
            }

            var str1 = productById.ProductAttributeMappings.Aggregate(string.Empty, (attributesXml, attribute) =>
            {
                foreach (var num in _productAttributeService.GetProductAttributeValues(attribute.Id)
                    .Where(v => v.IsPreSelected).Select(v => v.Id).ToList())
                    attributesXml =
                        _productAttributeParser.AddProductAttribute(attributesXml, attribute, num.ToString(),
                            new int?());
                return attributesXml;
            });
            var shoppingCartType = isAddToCartButton ? (ShoppingCartType) 1 : (ShoppingCartType) 2;
            var cart = new List<string>();
            if (packageId == 0)
            {
                cart = _shoppingCartService.AddToCart(_workContext.CurrentCustomer, productById, shoppingCartType,
                    _storeContext.CurrentStore.Id, str1, 0M, new DateTime?(), new DateTime?(), quantity, true).ToList();
            }
            else
            {
                cart = _shoppingCartService.AddToCartWithPackage(packageId, _workContext.CurrentCustomer, productById, shoppingCartType,
                    _storeContext.CurrentStore.Id, str1, 0M, new DateTime?(), new DateTime?(), quantity, true).ToList();
            }
            if (cart.Count == 0)
            {
                var cartWindowResult = ProductAddedToCartWindowResult(productById, quantity, isAddToCartButton);
                toCartResultModel.ProductAddedToCartWindow = cartWindowResult;
                return Json(toCartResultModel);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("<ul>");
            foreach (var str2 in cart)
                stringBuilder.AppendFormat("<li>{0}</li>", str2);
            stringBuilder.Append("</ul>");
            toCartResultModel.Status = "warning";
            toCartResultModel.PopupTitle =
                _localizationService.GetResource("SevenSpikes.NopAjaxCart.PopupTitle.Warning");
            toCartResultModel.AddToCartWarnings = stringBuilder.ToString();
            return Json(toCartResultModel);
        }

        private string ProductAddedToCartWindowResult(
            Product product,
            int quantity,
            bool isAddToCartButton)
        {
            if (isAddToCartButton)
                return RenderPartialViewToString("ProductAddedToCartPopupDialog",
                    new ProductAddedToCartPopupDialogModel()
                    {
                        MiniShoppingCart = GetShoppingCartItemModel(product.Id, quantity),
                        EnableRelatedProductsInPopup = _ajaxCartSettings.EnableRelatedProductsInPopup,
                        EnableCrossSellProductsInPopup = _ajaxCartSettings.EnableCrossSellProductsInPopup
                    });
            return RenderPartialViewToString("ProductAddedToWishlistPopupDialog",
                new ProductAddedToWishlistPopupDialogModel()
                {
                    WishlistShoppingCartItemModel = PrepareWishlistShoppingCartItemModel(product, quantity),
                    EnableRelatedProductsInPopup = _ajaxCartSettings.EnableRelatedProductsInPopup,
                    EnableCrossSellProductsInPopup = _ajaxCartSettings.EnableCrossSellProductsInPopup
                });
        }

        private WishlistModel.ShoppingCartItemModel PrepareWishlistShoppingCartItemModel(
            Product product,
            int quantity)
        {
            var shoppingCartItemModel1 = new WishlistModel.ShoppingCartItemModel();
            shoppingCartItemModel1.ProductId = product.Id;
            shoppingCartItemModel1.ProductName = _localizationService.GetLocalized(product, x => x.Name);
            /*Expression.Lambda<Func<Product, string>>((Expression) Expression.Property((Expression) parameterExpression, 
              (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (Product.get_Name))), parameterExpression), new int?(), true, true);*/

            shoppingCartItemModel1.ProductSeName = _urlRecordService.GetSeName(product, new int?(), true, true);
            shoppingCartItemModel1.Quantity = quantity;
            var shoppingCartItemModel2 = shoppingCartItemModel1;
            var shoppingCartItem = _workContext.CurrentCustomer.ShoppingCartItems.OrderByDescending(i => i.UpdatedOnUtc)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .FirstOrDefault(sci => (int) sci.ShoppingCartType == 2 && sci.Product == product);

            var num2 = _currencyService.ConvertFromPrimaryStoreCurrency(
                _taxService.GetProductPrice(product, _priceCalculationService.GetUnitPrice(shoppingCartItem, true),
                    out var num1), _workContext.WorkingCurrency);
            shoppingCartItemModel2.UnitPrice = _priceFormatter.FormatPrice(num2);
            shoppingCartItemModel2.Picture = _shoppingCartModelFactory.PrepareCartItemPictureModel(shoppingCartItem,
                _ajaxCartSettings.ProductAddedToCartImageSize, true, shoppingCartItemModel2.ProductName);
            shoppingCartItemModel2.AttributeInfo =
                _productAttributeFormatter.FormatAttributes(shoppingCartItem.Product, shoppingCartItem.AttributesXml);
            return shoppingCartItemModel2;
        }

        private JsonResult AddProductFromProductDetailsPageToCartAjaxInternal(
            int productId,
            bool isAddToCartButton,
            IFormCollection form)
        {
            var shoppingCartType = isAddToCartButton ? (ShoppingCartType) 1 : (ShoppingCartType) 2;
            if (!(AddProductToCart_Details(productId, (int) shoppingCartType, form) is JsonResult cartDetails))
                return new NullJsonResult();
            var toCartResultModel1 = new AddProductToCartResultModel()
            {
                Status = "success",
                AddToCartWarnings = string.Empty,
                ErrorMessage = string.Empty,
                RedirectUrl = string.Empty
            };
            var toCartResultModel2 =
                JsonConvert.DeserializeObject<AddProductVariantToCartResultModel>(
                    JsonConvert.SerializeObject(cartDetails.Value));
            if (toCartResultModel2.success)
            {
                var result = 1;
                foreach (var key in form.Keys)
                {
                    if (key.Equals($"addtocart_{productId}.EnteredQuantity",
                        StringComparison.InvariantCultureIgnoreCase))
                        int.TryParse(form[key], out result);
                    else if (key.Equals($"rental_start_date_{productId}",
                        StringComparison.InvariantCultureIgnoreCase))
                        GetDate(form[key]);
                    else if (key.Equals($"rental_end_date_{productId}",
                        StringComparison.InvariantCultureIgnoreCase))
                        GetDate(form[key]);
                }

                var productById = _productService.GetProductById(productId);
                if (productById == null)
                {
                    toCartResultModel1.Status = "error";
                    toCartResultModel1.PopupTitle =
                        _localizationService.GetResource("SevenSpikes.NopAjaxCart.PopupTitle.Error");
                    toCartResultModel1.ErrorMessage = $"Product with Id {productId} cannot be found";
                    return Json(toCartResultModel1);
                }

                var cartWindowResult = ProductAddedToCartWindowResult(productById, result, isAddToCartButton);
                toCartResultModel1.ProductAddedToCartWindow = cartWindowResult;
                return Json(toCartResultModel1);
            }

            var stringBuilder = new StringBuilder();
            switch (toCartResultModel2.message)
            {
                case string str1:
                    toCartResultModel1.Status = "error";
                    toCartResultModel1.PopupTitle =
                        _localizationService.GetResource("SevenSpikes.NopAjaxCart.PopupTitle.Error");
                    toCartResultModel1.AddToCartWarnings = str1;
                    break;
                case JArray jarray:
                    stringBuilder.Append("<ul>");
                    foreach (var jtoken in jarray)
                    {
                        var str = jtoken.ToString();
                        stringBuilder.AppendFormat("<li>{0}</li>", str);
                    }

                    stringBuilder.Append("</ul>");
                    toCartResultModel1.Status = "warning";
                    toCartResultModel1.PopupTitle =
                        _localizationService.GetResource("SevenSpikes.NopAjaxCart.PopupTitle.Warning");
                    toCartResultModel1.AddToCartWarnings = stringBuilder.ToString();
                    break;
            }

            return Json(toCartResultModel1);
        }

        private DateTime? GetDate(string formKey)
        {
            var nullable = new DateTime?();
            var cultureInfo = new CultureInfo("en-US");
            if (DateTime.TryParse(formKey, cultureInfo, DateTimeStyles.None, out var result))
                nullable = new DateTime?(result);
            return nullable;
        }
    }
}