using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Customization.Domain.Orders;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Customization.Orders;
using Nop.Services.ExportImport.Help;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Customization.Models.Report;
using Nop.Web.Areas.Admin.Factories;
using System;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Customization.Factories
{
    public class CustomReportModelFactory : ICustomReportModelFactory
    {
        #region Fields
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomeOrderService _customeOrderService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IVendorService _vendorService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;
        #endregion

        #region Ctor
        public CustomReportModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            ICustomeOrderService customeOrderService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            IVendorService vendorService,
            ILocalizationService localizationService,
            IProductService productService,
            IWorkContext workContext,
            CatalogSettings catalogSettings)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _customeOrderService = customeOrderService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _vendorService = vendorService;
            _localizationService = localizationService;
            _productService = productService;
            _workContext = workContext;
            _catalogSettings = catalogSettings;
        }
        #endregion

        #region Fields
        public virtual CustomReportSearchModel PrepareCustomReportsSearchModel(CustomReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareOrderStatuses(searchModel.AvailableOrderStatuses);
            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual List<CustomReportModel> PrepareCustomReportModel(out int totalCount, DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int orderStatus = 0, int productId = 0, int pageIndex = 0, int pageSize = int.MaxValue-1)
        {

            //int totalRecord = 0;
            //var orders = _customeOrderService.SearchOrderReport(out totalRecord,
            //    createdFromUtc,
            //    createdToUtc,
            //    orderStatus,
            //    productId,
            //    pageIndex,
            //    pageSize);

            var orders = _customeOrderService.SearchExportOrderReport(
               createdFromUtc,
               createdToUtc,
               orderStatus,
               productId,
               pageIndex,
               pageSize);

            //var reportData = JsonConvert.DeserializeObject<List<CustomReportModel>>(orders);
            
            var model = new List<CustomReportModel>();
            foreach (var order in orders)
            {
                var customReportModel = new CustomReportModel()
                {
                    CustomOrderNumber= order.CustomOrderNumber,
                    OrderStatusId = order.OrderStatusId,
                    OrderStatus = _localizationService.GetLocalizedEnum((OrderStatus)order.OrderStatusId),
                    OrderProduct = order.ProductName,
                    Vender = order.VendorNames,
                    CustomerName = order.CustomerName,
                    PhoneNumber = order.PhoneNumber,
                    CustomerAddress = order.CustomerAddress,
                    CustomerEmail  = order.CustomerEmail,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOn, DateTimeKind.Utc),
                    OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false),
                    CustomerId = order.CustomerId
                };
                model.Add(customReportModel);
            }
            totalCount = orders.TotalCount;
            return model;
        }

        /// <summary>
        /// Prepare custom report
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public virtual CustomReportListModel PrepareCustomReportListModel(CustomReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ? null
               : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, _dateTimeHelper.CurrentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);
            var product = _productService.GetProductById(searchModel.ProductId);
            var filterByProductId = product != null && (_workContext.CurrentVendor == null || product.VendorId == _workContext.CurrentVendor.Id)
                ? searchModel.ProductId : 0;

            var reportData = PrepareCustomReportModel(out int totalCount,
                startDateValue,
                endDateValue,
                searchModel.OrderStatusId,
                filterByProductId,
                searchModel.Page - 1,
                searchModel.PageSize);

            //prepare list model
            var model = new CustomReportListModel
            {
                Data = reportData,
                Total = totalCount
            };
            return model;
        }
        

        /// <summary>
        /// Export orders to XLSX
        /// </summary>
        /// <param name="orders">Orders</param>
        public virtual byte[] ExportCustomizeReportToXlsx(IList<CustomReportModel> reportModels)
        {
            //a vendor should have access only to part of order information
            var ignore = _workContext.CurrentVendor != null;

            //property array
            var properties = new[]
            {
                new PropertyByName<CustomReportModel>("Customer Id", p => p.CustomerId),
                new PropertyByName<CustomReportModel>("Order #", p => p.CustomOrderNumber),
                new PropertyByName<CustomReportModel>("Order status", p => p.OrderStatus),
                new PropertyByName<CustomReportModel>("Order product", p => p.OrderProduct),
                new PropertyByName<CustomReportModel>("Vender", p => p.Vender),
                new PropertyByName<CustomReportModel>("Customer name", p => p.CustomerName),
                new PropertyByName<CustomReportModel>("PhoneNumber", p => p.PhoneNumber),
                new PropertyByName<CustomReportModel>("CustomerAddress", p => p.CustomerAddress),
                new PropertyByName<CustomReportModel>("Customer email", p => p.CustomerEmail),
                new PropertyByName<CustomReportModel>("Created on", p => p.CreatedOn.ToString()),
                new PropertyByName<CustomReportModel>("Order total", p => p.OrderTotal)
            };

            return new PropertyManager<CustomReportModel>(properties, _catalogSettings).ExportToXlsx(reportModels);
        }
        #endregion
    }
}
