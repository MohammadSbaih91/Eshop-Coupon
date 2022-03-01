using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Tax;
using Nop.Services.Affiliates;
using Nop.Services.Authentication.External;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Areas.Admin.Models.ShoppingCart;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the customer model factory implementation
    /// </summary>
    public partial class CustomerModelFactoryOverride : CustomerModelFactory
    {
        #region Fields
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        #endregion

        #region Ctor

        public CustomerModelFactoryOverride(AddressSettings addressSettings,
                CustomerSettings customerSettings,
                DateTimeSettings dateTimeSettings,
                GdprSettings gdprSettings,
                IAclSupportedModelFactory aclSupportedModelFactory,
                IAddressAttributeFormatter addressAttributeFormatter,
                IAddressAttributeModelFactory addressAttributeModelFactory,
                IAffiliateService affiliateService,
                IBackInStockSubscriptionService backInStockSubscriptionService,
                IBaseAdminModelFactory baseAdminModelFactory,
                ICustomerActivityService customerActivityService,
                ICustomerAttributeParser customerAttributeParser,
                ICustomerAttributeService customerAttributeService,
                ICustomerService customerService,
                IDateTimeHelper dateTimeHelper,
                IExternalAuthenticationService externalAuthenticationService,
                IGdprService gdprService,
                IGenericAttributeService genericAttributeService,
                IGeoLookupService geoLookupService,
                ILocalizationService localizationService,
                INewsLetterSubscriptionService newsLetterSubscriptionService,
                IOrderService orderService,
                IPictureService pictureService,
                IPriceCalculationService priceCalculationService,
                IPriceFormatter priceFormatter,
                IProductAttributeFormatter productAttributeFormatter,
                IRewardPointService rewardPointService,
                IStoreContext storeContext,
                IStoreService storeService,
                ITaxService taxService,
                MediaSettings mediaSettings,
                RewardPointsSettings rewardPointsSettings,
                TaxSettings taxSettings)
            :base(addressSettings, customerSettings, dateTimeSettings, gdprSettings, aclSupportedModelFactory,
            addressAttributeFormatter, addressAttributeModelFactory, affiliateService, backInStockSubscriptionService,
            baseAdminModelFactory, customerActivityService, customerAttributeParser, customerAttributeService,
            customerService, dateTimeHelper, externalAuthenticationService, gdprService, genericAttributeService,
            geoLookupService, localizationService, newsLetterSubscriptionService, orderService, pictureService,
            priceCalculationService, priceFormatter, productAttributeFormatter, rewardPointService, storeContext,
            storeService, taxService, mediaSettings, rewardPointsSettings, taxSettings)
        {
            this._dateTimeHelper = dateTimeHelper;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._productAttributeFormatter = productAttributeFormatter;
            this._storeService = storeService;
            this._taxService = taxService;
        }

        #endregion

        #region Methods

        public override CustomerShoppingCartListModel PrepareCustomerShoppingCartListModel(
            CustomerShoppingCartSearchModel searchModel,
            Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get customer shopping cart
            var shoppingCart = customer.ShoppingCartItems
                .Where(item => item.ShoppingCartTypeId == searchModel.ShoppingCartTypeId).ToList();

            //prepare list model
            var model = new CustomerShoppingCartListModel
            {
                Data = shoppingCart.PaginationByRequestModel(searchModel).Select(item =>
                {
                    //fill in model values from the entity
                    var shoppingCartItemModel = new ShoppingCartItemModel
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        ProductName = item.Product.Name,
                        AttributeInfo = _productAttributeFormatter.FormatAttributes(item.Product, item.AttributesXml),
                        UnitPrice = _priceFormatter
                            .FormatPrice(_taxService.GetProductPrice(item.Product,
                                _priceCalculationService.GetUnitPrice(item),item.Quantity, out _, out _)),
                        Total = _priceFormatter
                            .FormatPrice(_taxService.GetProductPrice(item.Product,
                                _priceCalculationService.GetSubTotal(item),item.Quantity, out _, out _))
                    };

                    //convert dates to the user time
                    shoppingCartItemModel.UpdatedOn =
                        _dateTimeHelper.ConvertToUserTime(item.UpdatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    shoppingCartItemModel.Store = _storeService.GetStoreById(item.StoreId)?.Name ?? "Unknown";

                    return shoppingCartItemModel;
                }),
                Total = shoppingCart.Count
            };

            return model;
        }

        #endregion
    }
}