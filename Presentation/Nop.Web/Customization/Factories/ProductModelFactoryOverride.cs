using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Vendors;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Customization.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the product model factory
    /// </summary>
    public partial class ProductModelFactoryOverride : ProductModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly ICurrencyService _currencyService;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public ProductModelFactoryOverride(CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CustomerSettings customerSettings,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateRangeService dateRangeService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            IReviewTypeService reviewTypeService,
            ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            SeoSettings seoSettings,
            VendorSettings vendorSettings
            , ITaxCategoryService taxCategoryService
             , ISettingService settingService)
            : base(captchaSettings, catalogSettings, customerSettings, categoryService, currencyService,
                customerService, dateRangeService, dateTimeHelper, downloadService, localizationService,
                manufacturerService, permissionService, pictureService, priceCalculationService, priceFormatter,
                productAttributeParser, productAttributeService, productService, productTagService,
                productTemplateService, reviewTypeService, specificationAttributeService, cacheManager, storeContext,
                taxService, urlRecordService, vendorService, webHelper, workContext, mediaSettings, orderSettings,
                seoSettings, vendorSettings)

        {
            this._catalogSettings = catalogSettings;
            this._currencyService = currencyService;
            this._downloadService = downloadService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._pictureService = pictureService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeService = productAttributeService;
            this._productService = productService;
            this._cacheManager = cacheManager;
            this._storeContext = storeContext;
            this._taxService = taxService;
            this._urlRecordService = urlRecordService;
            this._webHelper = webHelper;
            this._workContext = workContext;
            this._mediaSettings = mediaSettings;
            this._orderSettings = orderSettings;
            this._specificationAttributeService = specificationAttributeService;
            this._taxCategoryService = taxCategoryService;
            this._settingService = settingService;
        }

        #endregion

        #region Utilities

        public override IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var models = new List<ProductOverviewModel>();
            foreach (var product in products)
            {
                var model = new ProductOverviewModel
                {
                    Id = product.Id,
                    TaxCategoryId = product.TaxCategoryId,
                    OneMonthDiscount = product.OneMonthDiscount,
                    DiscountDesc = _localizationService.GetLocalized(product, x => x.DiscountDesc),
                    SpecialPromotion = product.SpecialPromotion,
                    SpecialPromotionDesc = _localizationService.GetLocalized(product, x => x.SpecialPromotionDesc),

                    TaxDesc = _taxCategoryService.GetTaxCategoryById(product.TaxCategoryId)?.Name,
                    TaxRate = Convert.ToDecimal(_settingService.GetSetting("tax.taxprovider.fixedorbycountrystatezip.taxcategoryid" + product.TaxCategoryId.ToString())?.Value),

                    Name = _localizationService.GetLocalized(product, x => x.Name),
                    ShortDescription = _localizationService.GetLocalized(product, x => x.ShortDescription),
                    FullDescription = _localizationService.GetLocalized(product, x => x.FullDescription),
                    SeName = _urlRecordService.GetSeName(product),
                    Sku = product.Sku,
                    ProductType = product.ProductType,
                    ManageInventoryMethod = (product.ManageInventoryMethod != ManageInventoryMethod.DontManageStock),
                    PriceWithoutDiscount = product.Price.ToString("F"),

                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow)
                };


                //price
                if (preparePriceModel)
                {

                    model.ProductPrice = PrepareProductOverviewPriceModel(product, forceRedirectionAfterAddingToCart);
                }

                //picture
                if (preparePictureModel)
                {
                    model.DefaultPictureModel = PrepareProductOverviewPictureModel(product, productThumbPictureSize);
                }

                //specs
                if (prepareSpecificationAttributes)
                {
                    model.SpecificationAttributeModels = PrepareProductSpecificationModel(product);
                }

                //reviews
                model.ReviewOverviewModel = PrepareProductReviewOverviewModel(product);



                models.Add(model);
            }
            return models;
        }

        protected override void PrepareSimpleProductOverviewPriceModel(Product product,
            ProductOverviewModel.ProductPriceModel priceModel)
        {
            //add to cart button
            priceModel.DisableBuyButton = product.DisableBuyButton ||
                                          !_permissionService.Authorize(StandardPermissionProvider
                                              .EnableShoppingCart) ||
                                          !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

            //add to wishlist button
            priceModel.DisableWishlistButton = product.DisableWishlistButton ||
                                               !_permissionService.Authorize(StandardPermissionProvider
                                                   .EnableWishlist) ||
                                               !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);
            //compare products
            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

            //rental
            priceModel.IsRental = product.IsRental;

            //pre-order
            if (product.AvailableForPreOrder)
            {
                priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                                  product.PreOrderAvailabilityStartDateTimeUtc.Value >=
                                                  DateTime.UtcNow;
                priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
            }

            //prices
            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                if (product.CustomerEntersPrice)
                    return;

                if (product.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                     _workContext.OriginalCustomerIfImpersonated == null))
                {
                    //call for price
                    priceModel.OldPrice = null;
                    priceModel.Price = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    //prices
                    var minPossiblePriceWithoutDiscount = _priceCalculationService.GetFinalPrice(product,
                        _workContext.CurrentCustomer, includeDiscounts: false);

                    var minPossiblePriceWithDiscount = _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer,decimal.Zero,
                        true,1,out var discountAmount,out _);

                    if (product.HasTierPrices)
                    {
                        //calculate price for the maximum quantity if we have tier prices, and choose minimal
                        minPossiblePriceWithoutDiscount = Math.Min(minPossiblePriceWithoutDiscount,
                            _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer,
                                includeDiscounts: false, quantity: int.MaxValue));
                        minPossiblePriceWithDiscount = Math.Min(minPossiblePriceWithDiscount,
                            _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer,
                                includeDiscounts: true, quantity: int.MaxValue));
                    }

                    var oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, 1, out _, out _);
                    var finalPriceWithoutDiscountBase =
                        _taxService.GetProductPrice(product, minPossiblePriceWithoutDiscount, 1, out _, out _);
                    var finalPriceWithDiscountBase =
                        _taxService.GetProductPrice(product, minPossiblePriceWithDiscount, 1, out _, out _);

                    if (finalPriceWithDiscountBase > decimal.Zero) 
                        finalPriceWithDiscountBase -= discountAmount;
                    var oldPrice =
                        _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                    var finalPriceWithoutDiscount =
                        _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase,
                            _workContext.WorkingCurrency);
                    var finalPriceWithDiscount =
                        _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase,
                            _workContext.WorkingCurrency);

                    //do we have tier prices configured?
                    var tierPrices = new List<TierPrice>();
                    if (product.HasTierPrices)
                    {
                        tierPrices.AddRange(product.TierPrices.OrderBy(tp => tp.Quantity)
                            .FilterByStore(_storeContext.CurrentStore.Id)
                            .FilterForCustomer(_workContext.CurrentCustomer)
                            .FilterByDate()
                            .RemoveDuplicatedQuantities());
                    }

                    //When there is just one tier price (with  qty 1), there are no actual savings in the list.
                    var displayFromMessage =
                        tierPrices.Any() && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
                    if (displayFromMessage)
                    {
                        priceModel.OldPrice = null;
                        priceModel.Price = string.Format(_localizationService.GetResource("Products.PriceRangeFrom"),
                            _priceFormatter.FormatPrice(finalPriceWithDiscount));
                        priceModel.PriceValue = finalPriceWithDiscount;
                    }
                    else
                    {
                        var strikeThroughPrice = decimal.Zero;

                        if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                            strikeThroughPrice = oldPrice;

                        if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                            strikeThroughPrice = finalPriceWithoutDiscount;

                        if (strikeThroughPrice > decimal.Zero)
                            priceModel.OldPrice = _priceFormatter.FormatOldPrice(strikeThroughPrice);

                        priceModel.Price = _priceFormatter.FormatPrice(finalPriceWithDiscount);
                        priceModel.PriceValue = finalPriceWithDiscount;
                    }

                    if (product.IsRental)
                    {
                        //rental product
                        priceModel.OldPrice = _priceFormatter.FormatRentalProductPeriod(product, priceModel.OldPrice);
                        priceModel.Price = _priceFormatter.FormatRentalProductPeriod(product, priceModel.Price);
                    }

                    //property for German market
                    //we display tax/shipping info only with "shipping enabled" for this product
                    //we also ensure this it's not free shipping
                    priceModel.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductBoxes &&
                                                        product.IsShipEnabled && !product.IsFreeShipping;

                    //PAngV baseprice (used in Germany)
                    priceModel.BasePricePAngV = _priceFormatter.FormatBasePrice(product, finalPriceWithDiscount);
                }
            }
            else
            {
                //hide prices
                priceModel.OldPrice = null;
                priceModel.Price = null;
            }
        }

        protected override void PrepareGroupedProductOverviewPriceModel(Product product,
            ProductOverviewModel.ProductPriceModel priceModel)
        {
            var associatedProducts = _productService.GetAssociatedProducts(product.Id,
                _storeContext.CurrentStore.Id);

            //add to cart button (ignore "DisableBuyButton" property for grouped products)
            priceModel.DisableBuyButton =
                !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart) ||
                !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

            //add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
            priceModel.DisableWishlistButton =
                !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) ||
                !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

            //compare products
            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
            if (!associatedProducts.Any())
                return;

            //we have at least one associated product
            //compare products
            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
            //priceModel.AvailableForPreOrder = false;

            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                //find a minimum possible price
                decimal? minPossiblePrice = null;
                Product minPriceProduct = null;
                foreach (var associatedProduct in associatedProducts)
                {
                    var tmpMinPossiblePrice =
                        _priceCalculationService.GetFinalPrice(associatedProduct, _workContext.CurrentCustomer);

                    if (associatedProduct.HasTierPrices)
                    {
                        //calculate price for the maximum quantity if we have tier prices, and choose minimal
                        tmpMinPossiblePrice = Math.Min(tmpMinPossiblePrice,
                            _priceCalculationService.GetFinalPrice(associatedProduct, _workContext.CurrentCustomer,
                                quantity: int.MaxValue));
                    }

                    if (minPossiblePrice.HasValue && tmpMinPossiblePrice >= minPossiblePrice.Value)
                        continue;
                    minPriceProduct = associatedProduct;
                    minPossiblePrice = tmpMinPossiblePrice;
                }

                if (minPriceProduct == null || minPriceProduct.CustomerEntersPrice)
                    return;

                if (minPriceProduct.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                     _workContext.OriginalCustomerIfImpersonated == null))
                {
                    priceModel.OldPrice = null;
                    priceModel.Price = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    //calculate prices
                    var finalPriceBase =
                        _taxService.GetProductPrice(minPriceProduct, minPossiblePrice.Value, 1, out _, out _);
                    var finalPrice =
                        _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

                    priceModel.OldPrice = null;
                    priceModel.Price = string.Format(_localizationService.GetResource("Products.PriceRangeFrom"),
                        _priceFormatter.FormatPrice(finalPrice));
                    priceModel.PriceValue = finalPrice;

                    //PAngV baseprice (used in Germany)
                    priceModel.BasePricePAngV = _priceFormatter.FormatBasePrice(product, finalPriceBase);
                }
            }
            else
            {
                //hide prices
                priceModel.OldPrice = null;
                priceModel.Price = null;
            }
        }

        protected override ProductDetailsModel.ProductPriceModel PrepareProductPriceModel(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new ProductDetailsModel.ProductPriceModel
            {
                ProductId = product.Id
            };

            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                model.HidePrices = false;
                if (product.CustomerEntersPrice)
                {
                    model.CustomerEntersPrice = true;
                }
                else
                {
                    if (product.CallForPrice &&
                        //also check whether the current user is impersonated
                        (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                         _workContext.OriginalCustomerIfImpersonated == null))
                    {
                        model.CallForPrice = true;
                    }
                    else
                    {
                        var oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out _);
                        var finalPriceWithoutDiscountBase = _taxService.GetProductPrice(product,
                            _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer,
                                includeDiscounts: false), 1, out var taxRate, out var taxRate2);
                        var finalPriceWithDiscountBase = _taxService.GetProductPrice(product,
                            _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer,decimal.Zero,
                                 true,1,out var discountAmount,out var _), 1, out taxRate, out taxRate2);
                        model.TaxSplitInfo.IsTaxSpitEnable = product?.IsTaxSplitEnabled ?? false;
                        if (model.TaxSplitInfo.IsTaxSpitEnable)
                        {
                            model.TaxSplitInfo.SplitAmount = (decimal) product?.SplitAmount;
                            model.TaxSplitInfo.SplitAmount2 = (decimal) product?.SplitAmount2;
                            model.TaxSplitInfo.TaxSplit = taxRate;
                            model.TaxSplitInfo.TaxSplit2 = taxRate2;
                            model.TaxSplitInfo.DiscountAmount = discountAmount;
                        }

                        if (finalPriceWithDiscountBase> decimal.Zero) 
                            finalPriceWithDiscountBase -= discountAmount;

                        var oldPrice =
                            _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase,
                                _workContext.WorkingCurrency);
                        var finalPriceWithoutDiscount =
                            _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase,
                                _workContext.WorkingCurrency);
                        var finalPriceWithDiscount =
                            _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase,
                                _workContext.WorkingCurrency);

                        if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                            model.OldPrice = _priceFormatter.FormatPrice(oldPrice);

                        model.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);

                        if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                            model.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount);

                        model.PriceValue = finalPriceWithDiscount;

                        //property for German market
                        //we display tax/shipping info only with "shipping enabled" for this product
                        //we also ensure this it's not free shipping
                        model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductDetailsPage
                                                       && product.IsShipEnabled &&
                                                       !product.IsFreeShipping;

                        //PAngV baseprice (used in Germany)
                        model.BasePricePAngV = _priceFormatter.FormatBasePrice(product, finalPriceWithDiscountBase);
                        //currency code
                        model.CurrencyCode = _workContext.WorkingCurrency.CurrencyCode;

                        //rental
                        if (product.IsRental)
                        {
                            model.IsRental = true;
                            var priceStr = _priceFormatter.FormatPrice(finalPriceWithDiscount);
                            model.RentalPrice = _priceFormatter.FormatRentalProductPeriod(product, priceStr);
                        }
                    }
                }
            }
            else
            {
                model.HidePrices = true;
                model.OldPrice = null;
                model.Price = null;
            }

            return model;
        }

        protected override IList<ProductDetailsModel.ProductAttributeModel> PrepareProductAttributeModels(
            Product product, ShoppingCartItem updatecartitem)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //performance optimization
            //We cache a value indicating whether a product has attributes
            IList<ProductAttributeMapping> productAttributeMapping = null;
            var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_HAS_PRODUCT_ATTRIBUTES_KEY, product.Id);
            var hasProductAttributesCache = _cacheManager.Get(cacheKey, () =>
            {
                //no value in the cache yet
                //let's load attributes and cache the result (true/false)
                productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                return productAttributeMapping.Any();
            });
            if (hasProductAttributesCache && productAttributeMapping == null)
            {
                //cache indicates that the product has attributes
                //let's load them
                productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            }

            if (productAttributeMapping == null)
            {
                productAttributeMapping = new List<ProductAttributeMapping>();
            }

            var model = new List<ProductDetailsModel.ProductAttributeModel>();

            foreach (var attribute in productAttributeMapping)
            {
                var attributeModel = new ProductDetailsModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = _localizationService.GetLocalized(attribute.ProductAttribute, x => x.Name),
                    Description = _localizationService.GetLocalized(attribute.ProductAttribute, x => x.Description),
                    TextPrompt = _localizationService.GetLocalized(attribute, x => x.TextPrompt),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = updatecartitem != null ? null : attribute.DefaultValue,
                    HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml),
                    IsShowButton = attribute.IsShowButton
                };
                if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new ProductDetailsModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = _localizationService.GetLocalized(attributeValue, x => x.Name),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = attributeValue.IsPreSelected,
                            CustomerEntersQty = attributeValue.CustomerEntersQty,
                            Quantity = attributeValue.Quantity
                        };
                        attributeModel.Values.Add(valueModel);

                        //display price if allowed
                        if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                        {
                            var monthlyPriceAttrId = _settingService.GetSettingByKey<int>(EShopHelperService.MonthlyPriceAttribute);
                            var attributeValuePriceAdjustment = 0M;
                            var priceAdjustment = 0M;
                            if (attribute.ProductAttributeId == monthlyPriceAttrId)
                            {
                                attributeValuePriceAdjustment = attributeValue.PriceAdjustment;
                                priceAdjustment =
                                    _currencyService.ConvertFromPrimaryStoreCurrency(attributeValuePriceAdjustment,
                                        _workContext.WorkingCurrency);
                                valueModel.PriceAdjustment =
                                            "+" + _priceFormatter.FormatPrice(priceAdjustment, false, false);
                            }
                            else
                            {
                                attributeValuePriceAdjustment =
                                _priceCalculationService.GetProductAttributeValuePriceAdjustment(attributeValue,
                                    updatecartitem?.Customer ?? _workContext.CurrentCustomer);


                                var priceAdjustmentBase = _taxService.GetProductPrice(product,
                                    attributeValuePriceAdjustment, updatecartitem?.Quantity ?? 1, out _, out _);
                                priceAdjustment =
                                    _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase,
                                        _workContext.WorkingCurrency);

                                if (!TaxSplittingHelper.IsProductTaxSplittingOverridenByAttributeValue(attributeValue,
                                        product) && attributeValue.PriceAdjustmentUsePercentage)
                                {
                                    var priceAdjustmentStr = attributeValue.PriceAdjustment.ToString("G29");
                                    if (attributeValue.PriceAdjustment > decimal.Zero)
                                        valueModel.PriceAdjustment = "+";
                                    valueModel.PriceAdjustment += priceAdjustmentStr + "%";
                                }
                                else
                                {
                                    if (TaxSplittingHelper.IsProductTaxSplittingOverridenByAttributeValue(attributeValue, product))
                                        valueModel.PriceAdjustment =
                                            _priceFormatter.FormatPrice(priceAdjustment, false, false);

                                    else if (priceAdjustmentBase > decimal.Zero)
                                        valueModel.PriceAdjustment =
                                            "+" + _priceFormatter.FormatPrice(priceAdjustment, false, false);
                                    else if (priceAdjustmentBase < decimal.Zero)
                                        valueModel.PriceAdjustment =
                                            "-" + _priceFormatter.FormatPrice(-priceAdjustment, false, false);
                                }
                            }
                            valueModel.PriceAdjustmentValue = priceAdjustment;
                        }

                        //"image square" picture (with with "image squares" attribute type only)
                        if (attributeValue.ImageSquaresPictureId > 0)
                        {
                            var productAttributeImageSquarePictureCacheKey = string.Format(
                                ModelCacheEventConsumer.PRODUCTATTRIBUTE_IMAGESQUARE_PICTURE_MODEL_KEY,
                                attributeValue.ImageSquaresPictureId,
                                _webHelper.IsCurrentConnectionSecured(),
                                _storeContext.CurrentStore.Id);
                            valueModel.ImageSquaresPictureModel = _cacheManager.Get(
                                productAttributeImageSquarePictureCacheKey, () =>
                                {
                                    var imageSquaresPicture =
                                        _pictureService.GetPictureById(attributeValue.ImageSquaresPictureId);
                                    if (imageSquaresPicture != null)
                                    {
                                        return new PictureModel
                                        {
                                            FullSizeImageUrl = _pictureService.GetPictureUrl(imageSquaresPicture),
                                            ImageUrl = _pictureService.GetPictureUrl(imageSquaresPicture,
                                                _mediaSettings.ImageSquarePictureSize)
                                        };
                                    }

                                    return new PictureModel();
                                });
                        }

                        //picture of a product attribute value
                        valueModel.PictureId = attributeValue.PictureId;
                    }
                }

                //set already selected attributes (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                {
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                        {
                            if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues =
                                    _productAttributeParser.ParseProductAttributeValues(updatecartitem.AttributesXml);
                                foreach (var attributeValue in selectedValues)
                                foreach (var item in attributeModel.Values)
                                    if (attributeValue.Id == item.Id)
                                    {
                                        item.IsPreSelected = true;

                                        //set customer entered quantity
                                        if (attributeValue.CustomerEntersQty)
                                            item.Quantity = attributeValue.Quantity;
                                    }
                            }
                        }
                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //values are already pre-set

                            //set customer entered quantity
                            if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                            {
                                foreach (var attributeValue in _productAttributeParser
                                    .ParseProductAttributeValues(updatecartitem.AttributesXml)
                                    .Where(value => value.CustomerEntersQty))
                                {
                                    var item = attributeModel.Values.FirstOrDefault(value =>
                                        value.Id == attributeValue.Id);
                                    if (item != null)
                                        item.Quantity = attributeValue.Quantity;
                                }
                            }
                        }
                            break;
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        {
                            if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                            {
                                var enteredText =
                                    _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                            break;
                        case AttributeControlType.Datepicker:
                        {
                            //keep in mind my that the code below works only in the current culture
                            var selectedDateStr =
                                _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                            if (selectedDateStr.Any())
                            {
                                if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture,
                                    DateTimeStyles.None, out DateTime selectedDate))
                                {
                                    //successfully parsed
                                    attributeModel.SelectedDay = selectedDate.Day;
                                    attributeModel.SelectedMonth = selectedDate.Month;
                                    attributeModel.SelectedYear = selectedDate.Year;
                                }
                            }
                        }
                            break;
                        case AttributeControlType.FileUpload:
                        {
                            if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                            {
                                var downloadGuidStr = _productAttributeParser
                                    .ParseValues(updatecartitem.AttributesXml, attribute.Id).FirstOrDefault();
                                Guid.TryParse(downloadGuidStr, out Guid downloadGuid);
                                var download = _downloadService.GetDownloadByGuid(downloadGuid);
                                if (download != null)
                                    attributeModel.DefaultValue = download.DownloadGuid.ToString();
                            }
                        }
                            break;
                        default:
                            break;
                    }
                }

                model.Add(attributeModel);
            }

            return model;
        }

        protected override IList<ProductDetailsModel.TierPriceModel> PrepareProductTierPriceModels(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = product.TierPrices.OrderBy(x => x.Quantity)
                .FilterByStore(_storeContext.CurrentStore.Id)
                .FilterForCustomer(_workContext.CurrentCustomer)
                .FilterByDate()
                .RemoveDuplicatedQuantities()
                .Select(tierPrice =>
                {
                    var priceBase = _taxService.GetProductPrice(product, _priceCalculationService.GetFinalPrice(product,
                        _workContext.CurrentCustomer, decimal.Zero, _catalogSettings.DisplayTierPricesWithDiscounts,
                        tierPrice.Quantity), tierPrice.Quantity, out _, out _);
                    var price = _currencyService.ConvertFromPrimaryStoreCurrency(priceBase,
                        _workContext.WorkingCurrency);

                    return new ProductDetailsModel.TierPriceModel
                    {
                        Quantity = tierPrice.Quantity,
                        Price = _priceFormatter.FormatPrice(price, false, false)
                    };
                }).ToList();

            return model;
        }

        /// <summary>
        /// Prepare the product specification models
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of product specification model</returns>
        public override IList<ProductSpecificationModel> PrepareProductSpecificationModel(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, product.Id, _workContext.WorkingLanguage.Id);
            return _cacheManager.Get(cacheKey, () =>
                _specificationAttributeService.GetProductSpecificationAttributes(product.Id, 0, null, true)
                .Select(psa =>
                {
                    var picture = _pictureService.GetPictureById(psa.SpecificationAttributeOption.SpecificationAttribute.PictureId);
                    var m = new ProductSpecificationModel
                    {
                        SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
                        SpecificationAttributeName = _localizationService.GetLocalized(psa.SpecificationAttributeOption.SpecificationAttribute, x => x.Name),
                        ColorSquaresRgb = psa.SpecificationAttributeOption.ColorSquaresRgb,
                        AttributeTypeId = psa.AttributeTypeId,
                        PictureId = psa.SpecificationAttributeOption.SpecificationAttribute.PictureId,
                        IsShowInsideBox = psa.SpecificationAttributeOption.SpecificationAttribute.IsShowInsideBox,
                        PictureUrl = _pictureService.GetPictureUrl(picture),
                    };

                    switch (psa.AttributeType)
                    {
                        case SpecificationAttributeType.Option:
                            m.ValueRaw = WebUtility.HtmlEncode(_localizationService.GetLocalized(psa.SpecificationAttributeOption, x => x.Name));
                            break;
                        case SpecificationAttributeType.CustomText:
                            m.ValueRaw = WebUtility.HtmlEncode(psa.CustomValue);
                            break;
                        case SpecificationAttributeType.CustomHtmlText:
                            m.ValueRaw = psa.CustomValue;
                            break;
                        case SpecificationAttributeType.Hyperlink:
                            m.ValueRaw = $"<a href='{psa.CustomValue}' target='_blank'>{psa.CustomValue}</a>";
                            break;
                        default:
                            break;
                    }
                    return m;
                }).ToList()
            );
        }

        #endregion

        #region Methods
        public override ProductDetailsModel PrepareProductDetailsModel(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false)
        {
            var model = base.PrepareProductDetailsModel(product,updatecartitem,isAssociatedProduct);
            model.IsHidePlanSelection =product.IsHidePlanSelection;
            model.OfferDetailsCTA = _localizationService.GetLocalized(product, x => x.OfferDetailsCTA);
            model.KnowingTerms = _localizationService.GetLocalized(product, x => x.KnowingTerms);
            model.Conditions = _localizationService.GetLocalized(product, x => x.Conditions);
            model.ImportantNotes = _localizationService.GetLocalized(product, x => x.ImportantNotes);
            model.AddToCart.ValidateLocationBasedService = product.ValidateLocationBasedService;
            return model;
        }
        #endregion
    }
}