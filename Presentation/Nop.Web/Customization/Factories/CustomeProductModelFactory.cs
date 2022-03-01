using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Vendors;
using Nop.Web.Customization.Models.Catalog;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Factories
{
    public interface ICustomeProductModelFactory
    {
        IList<ProductAttributeOverviewModel> PrepareProductAttributeModels(Product product);
    }

    public class CustomeProductModelFactory : ICustomeProductModelFactory
    {
        #region fields
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        #endregion

        public CustomeProductModelFactory(
            ILocalizationService localizationService,
            IProductAttributeService productAttributeService,
            IStaticCacheManager cacheManager, IPriceFormatter priceFormatter, IWorkContext workContext, ICurrencyService currencyService)
        {
            this._localizationService = localizationService;
            this._productAttributeService = productAttributeService;
            this._cacheManager = cacheManager;
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
        }

        #region Utilities

        /// <summary>
        /// Prepare the product attribute models
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of product attribute model</returns>
        public virtual IList<ProductAttributeOverviewModel> PrepareProductAttributeModels(Product product)
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

            var model = new List<ProductAttributeOverviewModel>();

            foreach (var attribute in productAttributeMapping)
            {
                var attributeModel = new ProductAttributeOverviewModel
                {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = _localizationService.GetLocalized(attribute.ProductAttribute, x => x.Name),
                    TextPrompt = _localizationService.GetLocalized(attribute, x => x.TextPrompt),
                    AttributeControlType = attribute.AttributeControlType
                };
                
                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new ProductAttributeValueOverviewModel
                        {
                            Id = attributeValue.Id,
                            ProductAttributeMappingId = attribute.Id,
                            AttributeValueTypeId = attributeValue.AttributeValueTypeId,
                            Name = _localizationService.GetLocalized(attributeValue, x => x.Name),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = attributeValue.IsPreSelected,
                            Price = EShopHelper.GetPriceFormatting(_priceFormatter.FormatPrice(attributeValue.PriceAdjustment)),
                            PriceValue = _currencyService.ConvertFromPrimaryStoreCurrency(attributeValue.PriceAdjustment, _workContext.WorkingCurrency)

                        };
                        attributeModel.Values.Add(valueModel);

                    }
                }
                model.Add(attributeModel);
            }

            return model;
        }
        #endregion
    }
}
