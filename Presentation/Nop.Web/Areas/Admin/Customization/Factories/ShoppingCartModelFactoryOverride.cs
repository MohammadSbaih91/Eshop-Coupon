using System;
using System.Linq;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Models.ShoppingCart;
using Nop.Web.Framework.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the shopping cart model factory implementation
    /// </summary>
    public partial class ShoppingCartModelFactoryOverride : ShoppingCartModelFactory
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

        public ShoppingCartModelFactoryOverride(IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatter productAttributeFormatter,
            IStoreService storeService,
            ITaxService taxService)
            : base(baseAdminModelFactory, customerService, dateTimeHelper, localizationService, priceCalculationService,
                priceFormatter, productAttributeFormatter, storeService, taxService)
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

        public override ShoppingCartItemListModel PrepareShoppingCartItemListModel(
            ShoppingCartItemSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get shopping cart items
            var items = customer.ShoppingCartItems.Where(item => item.ShoppingCartType == searchModel.ShoppingCartType)
                .ToList();

            //prepare list model
            var model = new ShoppingCartItemListModel
            {
                Data = items.PaginationByRequestModel(searchModel).Select(item =>
                {
                    //fill in model values from the entity
                    var itemModel = new ShoppingCartItemModel
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        ProductName = item.Product?.Name
                    };

                    //convert dates to the user time
                    itemModel.UpdatedOn = _dateTimeHelper.ConvertToUserTime(item.UpdatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    itemModel.Store = _storeService.GetStoreById(item.StoreId)?.Name ?? "Deleted";
                    itemModel.AttributeInfo =
                        _productAttributeFormatter.FormatAttributes(item.Product, item.AttributesXml, item.Customer);
                    var unitPrice = _priceCalculationService.GetUnitPrice(item);
                    itemModel.UnitPrice =
                        _priceFormatter.FormatPrice(_taxService.GetProductPrice(item.Product, unitPrice, item.Quantity, out var _, out _));
                    var subTotal = _priceCalculationService.GetSubTotal(item);
                    itemModel.Total =
                        _priceFormatter.FormatPrice(_taxService.GetProductPrice(item.Product, subTotal, item.Quantity,out _, out _));

                    return itemModel;
                }),
                Total = items.Count
            };

            return model;
        }

        #endregion
    }
}