using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the product model factory implementation
    /// </summary>
    public partial class ProductModelFactoryOverride : ProductModelFactory
    {
        #region Fields
        private readonly IProductAttributeService _productAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IUrlRecordService _urlRecordService;
        #endregion

        #region Ctor
        public ProductModelFactoryOverride(CurrencySettings currencySettings,
            IAclSupportedModelFactory aclSupportedModelFactory, IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService, ICurrencyService currencyService, ICustomerService customerService,
            IDateTimeHelper dateTimeHelper, IDiscountService discountService,
            IDiscountSupportedModelFactory discountSupportedModelFactory, ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory, IManufacturerService manufacturerService,
            IMeasureService measureService, IOrderService orderService, IPictureService pictureService,
            IProductAttributeFormatter productAttributeFormatter, IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService, IProductService productService,
            IProductTagService productTagService, IProductTemplateService productTemplateService,
            ISettingModelFactory settingModelFactory, IShipmentService shipmentService,
            IShippingService shippingService, IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService, IStaticCacheManager cacheManager,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory, IStoreService storeService,
            IUrlRecordService urlRecordService, IWorkContext workContext, MeasureSettings measureSettings,
            TaxSettings taxSettings, VendorSettings vendorSettings)
            : base(currencySettings, aclSupportedModelFactory,
                baseAdminModelFactory, categoryService, currencyService, customerService, dateTimeHelper,
                discountService,
                discountSupportedModelFactory, localizationService, localizedModelFactory, manufacturerService,
                measureService, orderService, pictureService, productAttributeFormatter, productAttributeParser,
                productAttributeService, productService, productTagService, productTemplateService, settingModelFactory,
                shipmentService, shippingService, shoppingCartService, specificationAttributeService, cacheManager,
                storeMappingSupportedModelFactory, storeService, urlRecordService, workContext, measureSettings,
                taxSettings, vendorSettings)
        {
            _localizationService = localizationService;
            _pictureService = pictureService;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _localizedModelFactory = localizedModelFactory;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare product attribute value model
        /// </summary>
        /// <param name="model">Product attribute value model</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <param name="productAttributeValue">Product attribute value</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Product attribute value model</returns>
        public override ProductAttributeValueModel PrepareProductAttributeValueModel(ProductAttributeValueModel model,
            ProductAttributeMapping productAttributeMapping, ProductAttributeValue productAttributeValue,
            bool excludeProperties = false)
        {
            model = base.PrepareProductAttributeValueModel(model, productAttributeMapping, productAttributeValue,
                excludeProperties);

            if (productAttributeValue == null) return model;

            model.SplitAmount = productAttributeValue.SplitAmount;
            model.SplitAmount2 = productAttributeValue.SplitAmount2;

            return model;
        }

        /// <summary>
        /// Prepare paged product attribute value list model
        /// </summary>
        /// <param name="searchModel">Product attribute value search model</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <returns>Product attribute value list model</returns>
        public override ProductAttributeValueListModel PrepareProductAttributeValueListModel(ProductAttributeValueSearchModel searchModel,
            ProductAttributeMapping productAttributeMapping)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (productAttributeMapping == null)
                throw new ArgumentNullException(nameof(productAttributeMapping));

            //get product attribute values
            var productAttributeValues = _productAttributeService.GetProductAttributeValues(productAttributeMapping.Id);

            //prepare list model
            var model = new ProductAttributeValueListModel
            {
                Data = productAttributeValues.PaginationByRequestModel(searchModel).Select(value =>
                {
                    //fill in model values from the entity
                    var productAttributeValueModel = new ProductAttributeValueModel
                    {
                        Id = value.Id,
                        ProductAttributeMappingId = value.ProductAttributeMappingId,
                        AttributeValueTypeId = value.AttributeValueTypeId,
                        AssociatedProductId = value.AssociatedProductId,
                        ColorSquaresRgb = value.ColorSquaresRgb,
                        ImageSquaresPictureId = value.ImageSquaresPictureId,
                        PriceAdjustment = value.PriceAdjustment,
                        PriceAdjustmentUsePercentage = value.PriceAdjustmentUsePercentage,
                        WeightAdjustment = value.WeightAdjustment,
                        Cost = value.Cost,
                        CustomerEntersQty = value.CustomerEntersQty,
                        Quantity = value.Quantity,
                        IsPreSelected = value.IsPreSelected,
                        DisplayOrder = value.DisplayOrder,
                        PictureId = value.PictureId,
                        SplitAmount = value.SplitAmount,
                        SplitAmount2 = value.SplitAmount2
                    };

                    //fill in additional values (not existing in the entity)
                    productAttributeValueModel.AttributeValueTypeName = _localizationService.GetLocalizedEnum(value.AttributeValueType);
                    productAttributeValueModel.Name = value.ProductAttributeMapping.AttributeControlType != AttributeControlType.ColorSquares
                        ? value.Name : $"{value.Name} - {value.ColorSquaresRgb}";
                    if (value.AttributeValueType == AttributeValueType.Simple)
                    {
                        productAttributeValueModel.PriceAdjustmentStr = value.PriceAdjustment.ToString("G29");
                        if (value.PriceAdjustmentUsePercentage)
                            productAttributeValueModel.PriceAdjustmentStr += " %";
                        productAttributeValueModel.WeightAdjustmentStr = value.WeightAdjustment.ToString("G29");
                    }

                    if (value.AttributeValueType == AttributeValueType.AssociatedToProduct)
                    {
                        productAttributeValueModel
                            .AssociatedProductName = _productService.GetProductById(value.AssociatedProductId)?.Name ?? string.Empty;
                    }

                    var pictureThumbnailUrl = _pictureService.GetPictureUrl(value.PictureId, 75, false);
                    //little hack here. Grid is rendered wrong way with <img> without "src" attribute
                    if (string.IsNullOrEmpty(pictureThumbnailUrl))
                        pictureThumbnailUrl = _pictureService.GetPictureUrl(null, 1);
                    productAttributeValueModel.PictureThumbnailUrl = pictureThumbnailUrl;

                    return productAttributeValueModel;
                }),
                Total = productAttributeValues.Count
            };
            return model;
        }

        public override ProductAttributeMappingModel PrepareProductAttributeMappingModel(ProductAttributeMappingModel model,
            Product product, ProductAttributeMapping productAttributeMapping, bool excludeProperties = false)
        {
            model = base.PrepareProductAttributeMappingModel(model, product, productAttributeMapping, excludeProperties);
            if (productAttributeMapping != null)
                model.IsShowButton = productAttributeMapping.IsShowButton;

            return model;
        }


        /// <summary>
        /// Prepare product model
        /// </summary>
        /// <param name="model">Product model</param>
        /// <param name="product">Product</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Product model</returns>
        public override ProductModel PrepareProductModel(ProductModel model, Product product, bool excludeProperties = false)
        {
            Action<ProductLocalizedModel, int> localizedModelConfiguration = null;

            var overmodel = base.PrepareProductModel(model, product, excludeProperties);

            if (product != null)
            {
                localizedModelConfiguration = (locale, languageId) =>
            {
                locale.Name = _localizationService.GetLocalized(product, entity => entity.Name, languageId, false, false);
                locale.FullDescription = _localizationService.GetLocalized(product, entity => entity.FullDescription, languageId, false, false);
                locale.ShortDescription = _localizationService.GetLocalized(product, entity => entity.ShortDescription, languageId, false, false);
                locale.MetaKeywords = _localizationService.GetLocalized(product, entity => entity.MetaKeywords, languageId, false, false);
                locale.MetaDescription = _localizationService.GetLocalized(product, entity => entity.MetaDescription, languageId, false, false);
                locale.MetaTitle = _localizationService.GetLocalized(product, entity => entity.MetaTitle, languageId, false, false);
                locale.SeName = _urlRecordService.GetSeName(product, languageId, false, false);
                locale.OfferDetailsCTA = _localizationService.GetLocalized(product, entity => entity.OfferDetailsCTA, languageId, false, false);
                locale.KnowingTerms = _localizationService.GetLocalized(product, entity => entity.KnowingTerms, languageId, false, false);
                locale.Conditions = _localizationService.GetLocalized(product, entity => entity.Conditions, languageId, false, false);
                locale.ImportantNotes = _localizationService.GetLocalized(product, entity => entity.ImportantNotes, languageId, false, false);
                locale.DiscountDesc = _localizationService.GetLocalized(product, entity => entity.DiscountDesc, languageId, false, false);
                locale.SpecialPromotionDesc = _localizationService.GetLocalized(product, entity => entity.SpecialPromotionDesc, languageId, false, false);
            };
                //prepare localized models
                if (!excludeProperties)
                    overmodel.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);
            }

            
            return overmodel;
        }
        #endregion
    }
}