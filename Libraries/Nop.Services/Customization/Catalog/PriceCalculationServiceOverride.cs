using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Customization.Catalog;
using Nop.Services.Directory;
using Nop.Services.Discounts;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Price calculation service
    /// </summary>
    public partial class PriceCalculationServiceOverride : PriceCalculationService
    {
        #region Fields

        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductService _productService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public PriceCalculationServiceOverride(CatalogSettings catalogSettings, CurrencySettings currencySettings,
            ICategoryService categoryService, ICurrencyService currencyService, IDiscountService discountService,
            IManufacturerService manufacturerService, IProductAttributeParser productAttributeParser,
            IProductService productService, IStaticCacheManager cacheManager, IStoreContext storeContext,
            IWorkContext workContext, ShoppingCartSettings shoppingCartSettings, IProductAttributeService productAttributeService, ISettingService settingService) : base(catalogSettings,
            currencySettings, categoryService, currencyService, discountService, manufacturerService,
            productAttributeParser, productService, cacheManager, storeContext, workContext, shoppingCartSettings)
        {
            _catalogSettings = catalogSettings;
            _productAttributeParser = productAttributeParser;
            _productService = productService;
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _workContext = workContext;
            _shoppingCartSettings = shoppingCartSettings;
            _productAttributeService = productAttributeService;
            _settingService = settingService;
        }

        #endregion


        #region Utilites

      

        protected static bool OverrideProductTaxSplittingByAttributeValue(ProductAttributeValue value, Product product)
        {
            if (!TaxSplittingHelper.IsProductTaxSplittingOverridenByAttributeValue(value, product)) return false;
            var attrSplitAmount = value.SplitAmount;
            var attrSplitAmount2 = value.SplitAmount2;

            var splitAmountHasValue = attrSplitAmount != default(decimal);
            var splitAmount2HasValue = attrSplitAmount2 != default(decimal);

            if (splitAmountHasValue)
                product.SplitAmount = attrSplitAmount;

            if (splitAmount2HasValue)
                product.SplitAmount2 = attrSplitAmount2;

            if (value.ProductAttributeMapping.Product == null
                || ReferenceEquals(product, value.ProductAttributeMapping.Product)
                || !value.ProductAttributeMapping.Product.IsTaxSplitEnabled)
                return splitAmountHasValue || splitAmount2HasValue;

            if (splitAmountHasValue)
                value.ProductAttributeMapping.Product.SplitAmount = attrSplitAmount;

            if (splitAmount2HasValue)
                value.ProductAttributeMapping.Product.SplitAmount2 = attrSplitAmount2;

            return splitAmountHasValue || splitAmount2HasValue;
        }

        #endregion

        #region Methods

        public override decimal GetProductAttributeValuePriceAdjustment(ProductAttributeValue value, Customer customer,
            decimal? productPrice = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var adjustment = decimal.Zero;
            switch (value.AttributeValueType)
            {
                case AttributeValueType.Simple:
                    //simple attribute
                    var isTaxSplittingOverriden = OverrideProductTaxSplittingByAttributeValue(value, value.ProductAttributeMapping.Product);

                    if (value.PriceAdjustmentUsePercentage && !isTaxSplittingOverriden)
                    {
                        if (!productPrice.HasValue)
                            productPrice = GetFinalPrice(value.ProductAttributeMapping.Product, customer);

                        adjustment = (decimal) ((float) productPrice * (float) value.PriceAdjustment / 100f);
                    }
                    else
                    {
                        adjustment = value.PriceAdjustment;
                    }


                    break;
                case AttributeValueType.AssociatedToProduct:
                    //bundled product
                    var associatedProduct = _productService.GetProductById(value.AssociatedProductId);
                    if (associatedProduct != null)
                    {
                        adjustment = GetFinalPrice(associatedProduct, _workContext.CurrentCustomer) * value.Quantity;
                        OverrideProductTaxSplittingByAttributeValue(value, associatedProduct);
                    }

                    break;
            }

            return adjustment;
        }

        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="attributesXml">Product attributes (XML format)</param>
        /// <param name="customerEnteredPrice">Customer entered price (if specified)</param>
        /// <param name="rentalStartDate">Rental start date (null for not rental products)</param>
        /// <param name="rentalEndDate">Rental end date (null for not rental products)</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <returns>Shopping cart unit price (one item)</returns>
        public override decimal GetUnitPrice(Product product,
            Customer customer,
            ShoppingCartType shoppingCartType,
            int quantity,
            string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate, DateTime? rentalEndDate,
            bool includeDiscounts,
            out decimal discountAmount,
            out List<DiscountForCaching> appliedDiscounts)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            discountAmount = decimal.Zero;
            appliedDiscounts = new List<DiscountForCaching>();

            decimal finalPrice;

            var combination = _productAttributeParser.FindProductAttributeCombination(product, attributesXml);
            if (combination?.OverriddenPrice.HasValue ?? false)
            {
                finalPrice = GetFinalPrice(product,
                    customer,
                    combination.OverriddenPrice.Value,
                    decimal.Zero,
                    includeDiscounts,
                    quantity,
                    product.IsRental ? rentalStartDate : null,
                    product.IsRental ? rentalEndDate : null,
                    out discountAmount, out appliedDiscounts);
            }
            else
            {
                //summarize price of all attributes
                var attributesTotalPrice = decimal.Zero;
                decimal? attributesOverridenPrice = null;
                var attributeValues = _productAttributeParser.ParseProductAttributeValues(attributesXml);
                var productAttributeMappings = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                
                //Exclude monthly price attribute from product price
                var monthlyPriceAttrId = _settingService.GetSettingByKey<int>(EShopHelperService.MonthlyPriceAttribute);
                var productAttributeMappingsids = productAttributeMappings.Where(x => x.ProductAttributeId == monthlyPriceAttrId).Select(x => x.Id).ToList();
                attributeValues = attributeValues.Where(x => !productAttributeMappingsids.Contains(x.ProductAttributeMappingId)).ToList();

                //Exclude advance payment attribute price from product price
                var advancePriceAttrId = _settingService.GetSettingByKey<int>(EShopHelperService.AdvancedPaymentAmountAttribute);
                var advanceproductAttributeMappingsids = productAttributeMappings.Where(x => x.ProductAttributeId == advancePriceAttrId).Select(x => x.Id).ToList();
                attributeValues = attributeValues.Where(x => !advanceproductAttributeMappingsids.Contains(x.ProductAttributeMappingId)).ToList();

                if (attributeValues != null && attributeValues.Count > 0)
                {                    
                    foreach (var attributeValue in attributeValues.OrderBy(a => a.ProductAttributeMapping.DisplayOrder))
                    {
                        var valuePriceAdjustment = GetProductAttributeValuePriceAdjustment(attributeValue, customer,
                            product.CustomerEntersPrice ? (decimal?) customerEnteredPrice : null);
                        attributesTotalPrice += valuePriceAdjustment;

                        if (!OverrideProductTaxSplittingByAttributeValue(attributeValue, product)) continue;
                        attributesOverridenPrice = valuePriceAdjustment;
                        attributesTotalPrice = decimal.Zero;
                        break;
                    }

                    if (attributesOverridenPrice == null && product.IsTaxSplitEnabled)
                        attributesTotalPrice = decimal.Zero;
                }

                //get price of a product (with previously calculated price of all attributes)
                if (product.CustomerEntersPrice)
                {
                    finalPrice = customerEnteredPrice;
                }
                else
                {
                    int qty;
                    if (_shoppingCartSettings.GroupTierPricesForDistinctShoppingCartItems)
                    {
                        //the same products with distinct product attributes could be stored as distinct "ShoppingCartItem" records
                        //so let's find how many of the current products are in the cart
                        qty = customer.ShoppingCartItems
                            .Where(x => x.ProductId == product.Id)
                            .Where(x => x.ShoppingCartType == shoppingCartType)
                            .Sum(x => x.Quantity);
                        if (qty == 0)
                        {
                            qty = quantity;
                        }
                    }
                    else
                    {
                        qty = quantity;
                    }

                    finalPrice = GetFinalPrice(product,
                        customer,
                        attributesOverridenPrice,
                        attributesTotalPrice,
                        includeDiscounts,
                        qty,
                        product.IsRental ? rentalStartDate : null,
                        product.IsRental ? rentalEndDate : null,
                        out discountAmount, out appliedDiscounts);
                }
            }

            if (product.IsTaxSplitEnabled)
                return finalPrice;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                finalPrice = this.RoundPrice(finalPrice);

            return finalPrice;
        }
        
                /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="overriddenProductPrice">Overridden product price. If specified, then it'll be used instead of a product price. For example, used with product attribute combinations</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <param name="rentalStartDate">Rental period start date (for rental products)</param>
        /// <param name="rentalEndDate">Rental period end date (for rental products)</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <returns>Final price</returns>
        public override decimal GetFinalPrice(Product product,
            Customer customer,
            decimal? overriddenProductPrice,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            DateTime? rentalStartDate,
            DateTime? rentalEndDate,
            out decimal discountAmount,
            out List<DiscountForCaching> appliedDiscounts)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            discountAmount = decimal.Zero;
            appliedDiscounts = new List<DiscountForCaching>();

            var cacheKey = string.Format(NopCatalogDefaults.ProductPriceModelCacheKey,
                product.Id,
                overriddenProductPrice?.ToString(CultureInfo.InvariantCulture),
                additionalCharge.ToString(CultureInfo.InvariantCulture),
                includeDiscounts,
                quantity,
                string.Join(",", customer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var cacheTime = _catalogSettings.CacheProductPrices ? 60 : 0;
            //we do not cache price for rental products
            //otherwise, it can cause memory leaks (to store all possible date period combinations)
            if (product.IsRental)
                cacheTime = 0;
            var cachedPrice = _cacheManager.Get(cacheKey, () =>
            {
                var result = new ProductPriceForCaching();

                //initial price
                var price = overriddenProductPrice ?? (product.IsTaxSplitEnabled? product.SplitAmount + product.SplitAmount2: product.Price);

                //tier prices
                var tierPrice = _productService.GetPreferredTierPrice(product, customer, _storeContext.CurrentStore.Id, quantity);
                if (tierPrice != null)
                    price = tierPrice.Price;

                //additional charge
                //price = price + additionalCharge;

                //rental products
                if (product.IsRental)
                    if (rentalStartDate.HasValue && rentalEndDate.HasValue)
                        price = price * _productService.GetRentalPeriods(product, rentalStartDate.Value, rentalEndDate.Value);

                if (includeDiscounts)
                {
                    //discount
                    var tmpDiscountAmount = GetDiscountAmount(product, customer, price, out var tmpAppliedDiscounts);
//                    price = price - tmpDiscountAmount;

                    if (tmpAppliedDiscounts?.Any() ?? false)
                    {
                        result.AppliedDiscounts = tmpAppliedDiscounts;
                        result.AppliedDiscountAmount = tmpDiscountAmount;
                    }
                }

                if (price < decimal.Zero)
                    price = decimal.Zero;

                result.Price = price;
                return result;
            }, cacheTime);

            if (!includeDiscounts) 
                return cachedPrice.Price;

            if (!cachedPrice.AppliedDiscounts.Any())
                return cachedPrice.Price;

            appliedDiscounts.AddRange(cachedPrice.AppliedDiscounts);
            discountAmount = cachedPrice.AppliedDiscountAmount;

            return cachedPrice.Price;
        }

                
        /// <summary>
        /// Round
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <param name="roundingType">The rounding type</param>
        /// <returns>Rounded value</returns>
        public override decimal Round(decimal value, RoundingType roundingType) =>
            roundingType == RoundingType.Rounding001 ? value : base.Round(value, roundingType);

        #endregion
    }
}