using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Discounts;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order service
    /// </summary>
    public partial class OrderTotalCalculationServiceOverride : OrderTotalCalculationService
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IDiscountService _discountService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly IPaymentService _paymentService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly TaxSettings _taxSettings;

        #endregion

        #region Ctor

        public OrderTotalCalculationServiceOverride(CatalogSettings catalogSettings,
            ICheckoutAttributeParser checkoutAttributeParser,
            IDiscountService discountService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IPaymentService paymentService,
            IPriceCalculationService priceCalculationService,
            IRewardPointService rewardPointService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings,
            TaxSettings taxSettings)
            : base(catalogSettings, checkoutAttributeParser, discountService, genericAttributeService, giftCardService,
                paymentService, priceCalculationService, rewardPointService, shippingService, shoppingCartService,
                storeContext, taxService, workContext, rewardPointsSettings, shippingSettings, shoppingCartSettings,
                taxSettings)
        {
            this._catalogSettings = catalogSettings;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._discountService = discountService;
            this._genericAttributeService = genericAttributeService;
            this._giftCardService = giftCardService;
            this._paymentService = paymentService;
            this._priceCalculationService = priceCalculationService;
            this._rewardPointService = rewardPointService;
            this._rewardPointsSettings = rewardPointsSettings;
            this._shippingService = shippingService;
            this._shoppingCartService = shoppingCartService;
            this._storeContext = storeContext;
            this._taxService = taxService;
            this._workContext = workContext;
            this._shippingSettings = shippingSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._taxSettings = taxSettings;
        }

        #endregion


        #region Methods

        public override void GetShoppingCartSubTotal(IList<ShoppingCartItem> cart,
            bool includingTax,
            out decimal discountAmount, out List<DiscountForCaching> appliedDiscounts,
            out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount,
            out SortedDictionary<decimal, decimal> taxRates)
        {
            discountAmount = decimal.Zero;
            appliedDiscounts = new List<DiscountForCaching>();
            subTotalWithoutDiscount = decimal.Zero;
            subTotalWithDiscount = decimal.Zero;
            taxRates = new SortedDictionary<decimal, decimal>();

            if (!cart.Any())
                return;

            //get the customer 
            var customer = cart.FirstOrDefault(item => item.Customer != null)?.Customer;

            //sub totals
            var subTotalExclTaxWithoutDiscount = decimal.Zero;
            var subTotalInclTaxWithoutDiscount = decimal.Zero;
            foreach (var shoppingCartItem in cart)
            {
                var sciSubTotal = _priceCalculationService.GetSubTotal(shoppingCartItem, true, out discountAmount,
                    out var _, out var maximumDiscountQty);
                sciSubTotal -= discountAmount;
                var sciExclTax = _taxService.GetProductPrice(shoppingCartItem.Product, sciSubTotal,
                    shoppingCartItem.Quantity, false, customer, out var taxRate, out var taxRate2);
                var sciInclTax = _taxService.GetProductPrice(shoppingCartItem.Product, sciSubTotal,
                    shoppingCartItem.Quantity, true, customer, out taxRate, out taxRate2);

                sciExclTax -= shoppingCartItem.SubsidyDiscount;
                sciInclTax -= shoppingCartItem.SubsidyDiscount;

                if (shoppingCartItem.Product.IsTaxSplitEnabled)
                {
                    if (maximumDiscountQty != null)
                    {
                        var discountQty = maximumDiscountQty.Value;
                        var discountedPrice = sciInclTax * discountQty - discountAmount;
                        
                        var notDiscountedQuantity = shoppingCartItem.Quantity - discountQty;
                        var notDiscountedSciInclTax = sciInclTax * notDiscountedQuantity;
                        sciInclTax = discountedPrice + notDiscountedSciInclTax;
                    }
                    else
                    {
                        sciInclTax *= shoppingCartItem.Quantity;
                        sciInclTax -= discountAmount;
                    }
                }

                subTotalExclTaxWithoutDiscount += sciExclTax;
                subTotalInclTaxWithoutDiscount += sciInclTax;

                //tax rates
                var sciTax = sciInclTax - sciExclTax;
                if (sciTax <= decimal.Zero) continue;

                if (taxRate > decimal.Zero)
                {
                    if (!taxRates.ContainsKey(taxRate))
                        taxRates.Add(taxRate, sciTax);
                    else
                        taxRates[taxRate] = taxRates[taxRate] + sciTax;
                }

                //we will only add TaxRate2 in tax rates is taxRate1 is zero
                if (taxRate > decimal.Zero || taxRate2 <= decimal.Zero) continue;

                if (!taxRates.ContainsKey(taxRate2))
                    taxRates.Add(taxRate2, sciTax);
                else
                    taxRates[taxRate2] = taxRates[taxRate2] + sciTax;
            }

            //checkout attributes
            if (customer != null)
            {
                var checkoutAttributesXml = _genericAttributeService.GetAttribute<string>(customer,
                    NopCustomerDefaults.CheckoutAttributes, _storeContext.CurrentStore.Id);
                var attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttributesXml);
                if (attributeValues != null)
                {
                    foreach (var attributeValue in attributeValues)
                    {
                        var caExclTax =
                            _taxService.GetCheckoutAttributePrice(attributeValue, false, customer, out var taxRate);
                        var caInclTax =
                            _taxService.GetCheckoutAttributePrice(attributeValue, true, customer, out taxRate);
                        subTotalExclTaxWithoutDiscount += caExclTax;
                        subTotalInclTaxWithoutDiscount += caInclTax;

                        //tax rates
                        var caTax = caInclTax - caExclTax;
                        if (taxRate <= decimal.Zero || caTax <= decimal.Zero)
                            continue;

                        if (!taxRates.ContainsKey(taxRate))
                        {
                            taxRates.Add(taxRate, caTax);
                        }
                        else
                        {
                            taxRates[taxRate] = taxRates[taxRate] + caTax;
                        }
                    }
                }
            }

            //subtotal without discount
            subTotalWithoutDiscount = includingTax ? subTotalInclTaxWithoutDiscount : subTotalExclTaxWithoutDiscount;
            if (subTotalWithoutDiscount < decimal.Zero)
                subTotalWithoutDiscount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                subTotalWithoutDiscount = _priceCalculationService.RoundPrice(subTotalWithoutDiscount);

            //We calculate discount amount on order subtotal excl tax (discount first)
            //calculate discount amount ('Applied to order subtotal' discount)
            var discountAmountExclTax =
                GetOrderSubtotalDiscount(customer, subTotalExclTaxWithoutDiscount, out appliedDiscounts);
            if (subTotalExclTaxWithoutDiscount < discountAmountExclTax)
                discountAmountExclTax = subTotalExclTaxWithoutDiscount;
            var discountAmountInclTax = discountAmountExclTax;
            //subtotal with discount (excl tax)
            var subTotalExclTaxWithDiscount = subTotalExclTaxWithoutDiscount - discountAmountExclTax;
            var subTotalInclTaxWithDiscount = subTotalExclTaxWithDiscount;

            //add tax for shopping items & checkout attributes
            var tempTaxRates = new Dictionary<decimal, decimal>(taxRates);
            foreach (var kvp in tempTaxRates)
            {
                var taxRate = kvp.Key;
                var taxValue = kvp.Value;

                if (taxValue == decimal.Zero)
                    continue;

                //discount the tax amount that applies to subtotal items
                if (subTotalExclTaxWithoutDiscount > decimal.Zero)
                {
                    var discountTax = taxRates[taxRate] * (discountAmountExclTax / subTotalExclTaxWithoutDiscount);
                    discountAmountInclTax += discountTax;
                    taxValue = taxRates[taxRate] - discountTax;
                    if (_shoppingCartSettings.RoundPricesDuringCalculation)
                        taxValue = _priceCalculationService.RoundPrice(taxValue);
                    taxRates[taxRate] = taxValue;
                }

                //subtotal with discount (incl tax)
                subTotalInclTaxWithDiscount += taxValue;
            }

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                discountAmountInclTax = _priceCalculationService.RoundPrice(discountAmountInclTax);
                discountAmountExclTax = _priceCalculationService.RoundPrice(discountAmountExclTax);
            }

            if (includingTax)
            {
                subTotalWithDiscount = subTotalInclTaxWithDiscount;
                discountAmount = discountAmountInclTax;
            }
            else
            {
                subTotalWithDiscount = subTotalExclTaxWithDiscount;
                discountAmount = discountAmountExclTax;
            }

            if (subTotalWithDiscount < decimal.Zero)
                subTotalWithDiscount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                subTotalWithDiscount = _priceCalculationService.RoundPrice(subTotalWithDiscount);
        }

        #endregion
    }
}