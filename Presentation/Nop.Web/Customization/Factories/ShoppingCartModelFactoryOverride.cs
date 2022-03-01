using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Card;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Customization.Discounts;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Web.Factories
{
    /// <summary>
    ///     Represents the shopping cart model factory
    /// </summary>
    public partial class ShoppingCartModelFactoryOverride : ShoppingCartModelFactory
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ISimCardService _simCardService;
        private readonly CatalogSettings _catalogSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly CommonSettings _commonSettings;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IVendorService _vendorService;
        private readonly IPaymentService _paymentService;
        private readonly ICustomDiscountService _customDiscountService;
        #endregion

        #region Ctor

        public ShoppingCartModelFactoryOverride(AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            CustomerSettings customerSettings,
            IAddressModelFactory addressModelFactory,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDiscountService discountService,
            IDownloadService downloadService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentService paymentService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatter productAttributeFormatter,
            IProductService productService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            ISimCardService simCardService,
            ICustomDiscountService customDiscountService)
            : base(addressSettings, captchaSettings, catalogSettings, commonSettings, customerSettings,
                addressModelFactory, checkoutAttributeFormatter, checkoutAttributeParser, checkoutAttributeService,
                countryService, currencyService, customerService, discountService, downloadService,
                genericAttributeService, giftCardService, httpContextAccessor, localizationService,
                orderProcessingService, orderTotalCalculationService, paymentService, permissionService, pictureService,
                priceCalculationService, priceFormatter, productAttributeFormatter, productService, shippingService,
                shoppingCartService, stateProvinceService, cacheManager, storeContext, taxService, urlRecordService,
                vendorService, webHelper, workContext, mediaSettings, orderSettings, rewardPointsSettings,
                shippingSettings, shoppingCartSettings, taxSettings, vendorSettings)
        {
            _customerSettings = customerSettings;
            _checkoutAttributeService = checkoutAttributeService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _orderProcessingService = orderProcessingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeFormatter = productAttributeFormatter;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _taxService = taxService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _taxSettings = taxSettings;
            _simCardService = simCardService;
            _catalogSettings = catalogSettings;
            _vendorSettings = vendorSettings;
            _genericAttributeService = genericAttributeService;
            _commonSettings = commonSettings;
            _customerService = customerService;
            _discountService = discountService;
            _vendorService = vendorService;
            _paymentService = paymentService;
            _customDiscountService = customDiscountService;
        }

        #endregion

        #region Utilities

        protected override ShoppingCartModel.ShoppingCartItemModel PrepareShoppingCartItemModel(
            IList<ShoppingCartItem> cart, ShoppingCartItem sci, IList<Vendor> vendors)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (sci == null)
                throw new ArgumentNullException(nameof(sci));

            var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel
            {
                Id = sci.Id,
                Sku = _productService.FormatSku(sci.Product, sci.AttributesXml),
                VendorName = vendors.FirstOrDefault(v => v.Id == sci.Product.VendorId)?.Name ?? string.Empty,
                ProductId = sci.Product.Id,
                ProductName = _localizationService.GetLocalized(sci.Product, x => x.Name),
                ProductSeName = _urlRecordService.GetSeName(sci.Product),
                Quantity = sci.Quantity,
                AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.Product, sci.AttributesXml,
                    _workContext.CurrentCustomer, renderPrices: false),
                PackageId = sci.PackageId,
            };

            //allow editing?
            //1. setting enabled?
            //2. simple product?
            //3. has attribute or gift card?
            //4. visible individually?
            cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                                             sci.Product.ProductType == ProductType.SimpleProduct &&
                                             (!string.IsNullOrEmpty(cartItemModel.AttributeInfo) ||
                                              sci.Product.IsGiftCard) &&
                                             sci.Product.VisibleIndividually;

            //disable removal?
            //1. do other items require this one?
            cartItemModel.DisableRemoval = cart.Any(item =>
                item.Product.RequireOtherProducts &&
                _productService.ParseRequiredProductIds(item.Product).Contains(sci.ProductId));

            #region RequireAnyOneFromOtherProducts

            //cart should have at least 1 of any one required from products
            var isLastOneRequiredProductInCart = cart.Any(item =>
            {
                var requiredProductIds = _productService.ParseRequiredAnyOneFromOtherProductIds(item.Product);
                return requiredProductIds.Contains(sci.Product.Id) 
                       && !cart.Any(i => requiredProductIds.Where(id => id != sci.Product.Id).Contains(i.Product.Id));
            });

            cartItemModel.DisableRemoval = cartItemModel.DisableRemoval || isLastOneRequiredProductInCart;

            #endregion
                
            //allowed quantities
            var allowedQuantities = _productService.ParseAllowedQuantities(sci.Product);
            foreach (var qty in allowedQuantities)
                cartItemModel.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = sci.Quantity == qty
                });

            //recurring info
            if (sci.Product.IsRecurring)
                cartItemModel.RecurringInfo = string.Format(
                    _localizationService.GetResource("ShoppingCart.RecurringPeriod"),
                    sci.Product.RecurringCycleLength,
                    _localizationService.GetLocalizedEnum(sci.Product.RecurringCyclePeriod));

            //rental info
            if (sci.Product.IsRental)
            {
                var rentalStartDate = sci.RentalStartDateUtc.HasValue
                    ? _productService.FormatRentalDate(sci.Product, sci.RentalStartDateUtc.Value)
                    : "";
                var rentalEndDate = sci.RentalEndDateUtc.HasValue
                    ? _productService.FormatRentalDate(sci.Product, sci.RentalEndDateUtc.Value)
                    : "";
                cartItemModel.RentalInfo =
                    string.Format(_localizationService.GetResource("ShoppingCart.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
            }

            //unit prices
            if (sci.Product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                 _workContext.OriginalCustomerIfImpersonated == null))
            {
                cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
            }
            else
            {
                var shoppingCartUnitPriceWithoutDiscountBase = _taxService.GetProductPrice(sci.Product,
                    _priceCalculationService.GetUnitPrice(sci, true, out var discountAmount, out var appliedDiscounts),
                    cartItemModel.Quantity, out var taxRate, out var taxRate2);
                
                cartItemModel.TaxSplitInfo.IsTaxSpitEnable = sci?.Product?.IsTaxSplitEnabled ?? false;
                if (cartItemModel.TaxSplitInfo.IsTaxSpitEnable)
                {
                    cartItemModel.TaxSplitInfo.SplitAmount = (decimal) sci.Product?.SplitAmount;
                    cartItemModel.TaxSplitInfo.SplitAmount2 = (decimal) sci.Product?.SplitAmount2;
                    cartItemModel.TaxSplitInfo.TaxSplit = taxRate;
                    cartItemModel.TaxSplitInfo.TaxSplit2 = taxRate2;
                    cartItemModel.TaxSplitInfo.DiscountAmount = discountAmount;
                }
                var shoppingCartUnitPriceWithDiscountBase =
                    shoppingCartUnitPriceWithoutDiscountBase <= decimal.Zero
                    ? decimal.Zero
                    :shoppingCartUnitPriceWithoutDiscountBase - discountAmount;
                
                var shoppingCartUnitPriceWithDiscount =
                    _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase,
                        _workContext.WorkingCurrency);
                cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
            }

            //subtotal, discount
            if (sci.Product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                 _workContext.OriginalCustomerIfImpersonated == null))
            {
                cartItemModel.SubTotal = _localizationService.GetResource("Products.CallForPrice");
            }
            else
            {
                //sub total
                var sciSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.Product,
                    _priceCalculationService.GetSubTotal(sci, true, out var shoppingCartItemDiscountBase, out _,
                        out var maximumDiscountQty), sci.Quantity,
                    out _, out _);

                cartItemModel.TaxSplitInfo.MaxDiscountQty = maximumDiscountQty ?? 0;
                
                if (sci.Product.IsTaxSplitEnabled && _taxSettings.TaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    sciSubTotalWithDiscountBase *= sci.Quantity;
                }

                sciSubTotalWithDiscountBase -= sciSubTotalWithDiscountBase <= decimal.Zero? decimal.Zero : shoppingCartItemDiscountBase;
                var shoppingCartItemSubTotalWithDiscount =
                    _currencyService.ConvertFromPrimaryStoreCurrency(sciSubTotalWithDiscountBase,
                        _workContext.WorkingCurrency);
                cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount - sci.SubsidyDiscount);
                cartItemModel.SubTotalValue = shoppingCartItemSubTotalWithDiscount - sci.SubsidyDiscount;
                cartItemModel.MaximumDiscountedQty = maximumDiscountQty;
                cartItemModel.SimCardId = sci.SimCardId;
                cartItemModel.SimCardNumber = _simCardService.GetSimCardById(sci.SimCardId)?.CardNumber;

                //display an applied discount amount
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    var shoppingCartItemDiscount =
                        _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase,
                            _workContext.WorkingCurrency);
                    cartItemModel.Discount =  _priceFormatter.FormatPrice(shoppingCartItemDiscount, true, _workContext.WorkingCurrency.CurrencyCode,false,_workContext.WorkingLanguage);
                }
            }

            //picture
            if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
                cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                    _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);

            //item warnings
            var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(
                _workContext.CurrentCustomer,
                sci.ShoppingCartType,
                sci.Product,
                sci.StoreId,
                sci.AttributesXml,
                sci.CustomerEnteredPrice,
                sci.RentalStartDateUtc,
                sci.RentalEndDateUtc,
                sci.Quantity,
                false,
                sci.Id);
            foreach (var warning in itemWarnings)
                cartItemModel.Warnings.Add(warning);

            return cartItemModel;
        }


        protected override WishlistModel.ShoppingCartItemModel PrepareWishlistItemModel(IList<ShoppingCartItem> cart,
            ShoppingCartItem sci)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (sci == null)
                throw new ArgumentNullException(nameof(sci));

            var cartItemModel = new WishlistModel.ShoppingCartItemModel
            {
                Id = sci.Id,
                Sku = _productService.FormatSku(sci.Product, sci.AttributesXml),
                ProductId = sci.Product.Id,
                ProductName = _localizationService.GetLocalized(sci.Product, x => x.Name),
                ProductSeName = _urlRecordService.GetSeName(sci.Product),
                Quantity = sci.Quantity,
                AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.Product, sci.AttributesXml)
            };

            //allow editing?
            //1. setting enabled?
            //2. simple product?
            //3. has attribute or gift card?
            //4. visible individually?
            cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                                             sci.Product.ProductType == ProductType.SimpleProduct &&
                                             (!string.IsNullOrEmpty(cartItemModel.AttributeInfo) ||
                                              sci.Product.IsGiftCard) &&
                                             sci.Product.VisibleIndividually;

            //allowed quantities
            var allowedQuantities = _productService.ParseAllowedQuantities(sci.Product);
            foreach (var qty in allowedQuantities)
                cartItemModel.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = sci.Quantity == qty
                });

            //recurring info
            if (sci.Product.IsRecurring)
                cartItemModel.RecurringInfo = string.Format(
                    _localizationService.GetResource("ShoppingCart.RecurringPeriod"),
                    sci.Product.RecurringCycleLength,
                    _localizationService.GetLocalizedEnum(sci.Product.RecurringCyclePeriod));

            //rental info
            if (sci.Product.IsRental)
            {
                var rentalStartDate = sci.RentalStartDateUtc.HasValue
                    ? _productService.FormatRentalDate(sci.Product, sci.RentalStartDateUtc.Value)
                    : "";
                var rentalEndDate = sci.RentalEndDateUtc.HasValue
                    ? _productService.FormatRentalDate(sci.Product, sci.RentalEndDateUtc.Value)
                    : "";
                cartItemModel.RentalInfo =
                    string.Format(_localizationService.GetResource("ShoppingCart.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
            }

            //unit prices
            if (sci.Product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                 _workContext.OriginalCustomerIfImpersonated == null))
            {
                cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
            }
            else
            {
                var shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Product,
                    _priceCalculationService.GetUnitPrice(sci), sci.Quantity, out var taxRate, out var taxRate2);
                var shoppingCartUnitPriceWithDiscount =
                    _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase,
                        _workContext.WorkingCurrency);
                cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                cartItemModel.TaxSplitInfo.IsTaxSpitEnable = sci?.Product?.IsTaxSplitEnabled ?? false;
                if (cartItemModel.TaxSplitInfo.IsTaxSpitEnable)
                {
                    cartItemModel.TaxSplitInfo.SplitAmount = (decimal) sci.Product?.SplitAmount;
                    cartItemModel.TaxSplitInfo.SplitAmount2 = (decimal) sci.Product?.SplitAmount2;
                    cartItemModel.TaxSplitInfo.TaxSplit = taxRate;
                    cartItemModel.TaxSplitInfo.TaxSplit2 = taxRate2;
                }
            }

            //subtotal, discount
            if (sci.Product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                 _workContext.OriginalCustomerIfImpersonated == null))
            {
                cartItemModel.SubTotal = _localizationService.GetResource("Products.CallForPrice");
            }
            else
            {
                //sub total
                var shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.Product,
                    _priceCalculationService.GetSubTotal(sci, true, out var shoppingCartItemDiscountBase,
                        out _, out var maximumDiscountQty), sci.Quantity,
                    out var taxRate, out var taxRate2);

                if (sci.Product.IsTaxSplitEnabled && _taxSettings.TaxDisplayType == TaxDisplayType.IncludingTax)
                    shoppingCartItemSubTotalWithDiscountBase *= sci.Quantity;
                var shoppingCartItemSubTotalWithDiscount =
                    _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase,
                        _workContext.WorkingCurrency);
                cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);
                cartItemModel.MaximumDiscountedQty = maximumDiscountQty;

                //display an applied discount amount
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    var shoppingCartItemDiscount =
                        _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase,
                            _workContext.WorkingCurrency);
                    cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                }
            }

            //picture
            if (_shoppingCartSettings.ShowProductImagesOnWishList)
                cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                    _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);

            //item warnings
            var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(
                _workContext.CurrentCustomer,
                sci.ShoppingCartType,
                sci.Product,
                sci.StoreId,
                sci.AttributesXml,
                sci.CustomerEnteredPrice,
                sci.RentalStartDateUtc,
                sci.RentalEndDateUtc,
                sci.Quantity,
                false,
                sci.Id);
            foreach (var warning in itemWarnings)
                cartItemModel.Warnings.Add(warning);

            return cartItemModel;
        }


        public override MiniShoppingCartModel PrepareMiniShoppingCartModel()
        {
            var model = new MiniShoppingCartModel
            {
                ShowProductImages = _shoppingCartSettings.ShowProductImagesInMiniShoppingCart,
                //let's always display it
                DisplayShoppingCartButton = true,
                CurrentCustomerIsGuest = _workContext.CurrentCustomer.IsGuest(),
                AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed
            };

            //performance optimization (use "HasShoppingCartItems" property)
            if (_workContext.CurrentCustomer.HasShoppingCartItems)
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                model.TotalProducts = cart.Sum(item => item.Quantity);
                if (cart.Any())
                {
                    //subtotal
                    var subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax &&
                                               !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                    _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax, out _,
                        out _, out var subTotalWithoutDiscountBase, out _);
                    var subtotalBase = subTotalWithoutDiscountBase;
                    var subtotal =
                        _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
                    model.SubTotal = _priceFormatter.FormatPrice(subtotal, false, _workContext.WorkingCurrency,
                        _workContext.WorkingLanguage, subTotalIncludingTax);

                    var requiresShipping = _shoppingCartService.ShoppingCartRequiresShipping(cart);
                    //a customer should visit the shopping cart page (hide checkout button) before going to checkout if:
                    //1. "terms of service" are enabled
                    //2. min order sub-total is OK
                    //3. we have at least one checkout attribute
                    var checkoutAttributesExistCacheKey = string.Format(
                        ModelCacheEventConsumer.CHECKOUTATTRIBUTES_EXIST_KEY,
                        _storeContext.CurrentStore.Id, requiresShipping);
                    var checkoutAttributesExist = _cacheManager.Get(checkoutAttributesExistCacheKey,
                        () =>
                        {
                            var checkoutAttributes =
                                _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id,
                                    !requiresShipping);
                            return checkoutAttributes.Any();
                        });

                    var minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
                    var downloadableProductsRequireRegistration =
                        _customerSettings.RequireRegistrationForDownloadableProducts &&
                        cart.Any(sci => sci.Product.IsDownload);

                    model.DisplayCheckoutButton = !_orderSettings.TermsOfServiceOnShoppingCartPage &&
                                                  minOrderSubtotalAmountOk &&
                                                  !checkoutAttributesExist &&
                                                  !(downloadableProductsRequireRegistration
                                                    && _workContext.CurrentCustomer.IsGuest());

                    //products. sort descending (recently added products)
                    foreach (var sci in cart
                        .OrderByDescending(x => x.Id)
                        .Take(_shoppingCartSettings.MiniShoppingCartProductNumber)
                        .ToList())
                    {
                        var cartItemModel = new MiniShoppingCartModel.ShoppingCartItemModel
                        {
                            Id = sci.Id,
                            ProductId = sci.Product.Id,
                            ProductName = _localizationService.GetLocalized(sci.Product, x => x.Name),
                            ProductSeName = _urlRecordService.GetSeName(sci.Product),
                            Quantity = sci.Quantity,
                            AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.Product, sci.AttributesXml,
                                _workContext.CurrentCustomer, renderPrices: false)
                        };

                        //unit prices
                        if (sci.Product.CallForPrice &&
                            //also check whether the current user is impersonated
                            (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                             _workContext.OriginalCustomerIfImpersonated == null))
                        {
                            cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                        }
                        else
                        {
                            var shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Product,
                                _priceCalculationService.GetUnitPrice(sci), sci.Quantity, out var taxRate,
                                out var taxRate2);
                            var shoppingCartUnitPriceWithDiscount =
                                _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase,
                                    _workContext.WorkingCurrency);
                            cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                            cartItemModel.TaxSplitInfo.IsTaxSpitEnable = sci?.Product?.IsTaxSplitEnabled ?? false;
                            if (cartItemModel.TaxSplitInfo.IsTaxSpitEnable)
                            {
                                cartItemModel.TaxSplitInfo.SplitAmount = (decimal) sci.Product?.SplitAmount;
                                cartItemModel.TaxSplitInfo.SplitAmount2 = (decimal) sci.Product?.SplitAmount2;
                                cartItemModel.TaxSplitInfo.TaxSplit = taxRate;
                                cartItemModel.TaxSplitInfo.TaxSplit2 = taxRate2;
                                cartItemModel.TaxSplitInfo.TaxSplit2 = taxRate2;
                            }
                        }

                        //picture
                        if (_shoppingCartSettings.ShowProductImagesInMiniShoppingCart)
                            cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                                _mediaSettings.MiniCartThumbPictureSize, true, cartItemModel.ProductName);

                        model.Items.Add(cartItemModel);
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// Prepare the shopping cart model
        /// </summary>
        /// <param name="model">Shopping cart model</param>
        /// <param name="cart">List of the shopping cart item</param>
        /// <param name="isEditable">Whether model is editable</param>
        /// <param name="validateCheckoutAttributes">Whether to validate checkout attributes</param>
        /// <param name="prepareAndDisplayOrderReviewData">Whether to prepare and display order review data</param>
        /// <returns>Shopping cart model</returns>
        public override ShoppingCartModel PrepareShoppingCartModel(ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true,
            bool validateCheckoutAttributes = false,
            bool prepareAndDisplayOrderReviewData = false)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //simple properties
            model.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;
            if (!cart.Any())
                return model;
            model.IsEditable = isEditable;
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
            model.ShowVendorName = _vendorSettings.ShowVendorOnOrderDetailsPage;
            var checkoutAttributesXml = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                NopCustomerDefaults.CheckoutAttributes, _storeContext.CurrentStore.Id);
            var minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
            if (!minOrderSubtotalAmountOk)
            {
                var minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                model.MinOrderSubtotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false));
            }
            model.TermsOfServiceOnShoppingCartPage = _orderSettings.TermsOfServiceOnShoppingCartPage;
            model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
            model.TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks;
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoShoppingCart;

            //discount and gift card boxes
            model.DiscountBox.Display = _shoppingCartSettings.ShowDiscountBox;
            var discountCouponCodes = _customerService.ParseAppliedDiscountCouponCodes(_workContext.CurrentCustomer);
            foreach (var couponCode in discountCouponCodes)
            {
                var discount = _discountService.GetAllDiscountsForCaching(couponCode: couponCode)
                    .FirstOrDefault(d => d.RequiresCouponCode && _discountService.ValidateDiscount(d, _workContext.CurrentCustomer).IsValid);

                if (discount != null)
                {
                    var discountCouponCode = _customDiscountService.GetCouponCodeByDiscountCode(couponCode);

                    model.DiscountBox.AppliedDiscountsWithCodes.Add(new ShoppingCartModel.DiscountBoxModel.DiscountInfoModel()
                    {
                        Id = discountCouponCode.DiscountId,
                        CouponCode = discountCouponCode.CouponCode
                    });
                }
            }
            model.GiftCardBox.Display = _shoppingCartSettings.ShowGiftCardBox;

            //cart warnings
            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, validateCheckoutAttributes);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            //checkout attributes
            model.CheckoutAttributes = PrepareCheckoutAttributeModels(cart);

            var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? _vendorService.GetVendorsByIds(cart.Select(item => item.Product.VendorId).ToArray()) : new List<Vendor>();

            //cart items
            foreach (var sci in cart)
            {
                var cartItemModel = PrepareShoppingCartItemModel(cart, sci, vendors);
                model.Items.Add(cartItemModel);
            }

            //payment methods
            //all payment methods (do not filter by country here as it could be not specified yet)
            var paymentMethods = _paymentService
                .LoadActivePaymentMethods(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id)
                .Where(pm => !pm.HidePaymentMethod(cart))
                .ToList();
            //payment methods displayed during checkout (not with "Button" type)
            var nonButtonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType != PaymentMethodType.Button)
                .ToList();
            //"button" payment methods(*displayed on the shopping cart page)
            var buttonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .ToList();
            foreach (var pm in buttonPaymentMethods)
            {
                if (_shoppingCartService.ShoppingCartIsRecurring(cart) && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                var viewComponentName = pm.GetPublicViewComponentName();
                model.ButtonPaymentMethodViewComponentNames.Add(viewComponentName);
            }
            //hide "Checkout" button if we have only "Button" payment methods
            model.HideCheckoutButton = !nonButtonPaymentMethods.Any() && model.ButtonPaymentMethodViewComponentNames.Any();

            //order review data
            if (prepareAndDisplayOrderReviewData)
            {
                model.OrderReviewData = PrepareOrderReviewDataModel(cart);
            }

            return model;
        }

        #endregion
    }
}