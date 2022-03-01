using Nop.Core;
using Nop.Core.Customization.Domain.Orders;
using Nop.Core.Data;
using Nop.Core.Data.Extensions;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Customization.Orders
{
    public class CustomeOrderService : ICustomeOrderService
    {
        #region Fields
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerService _customerService;
        private readonly IDbContext _dbContext;
        private readonly IDataProvider _dataProvider;
        #endregion

        #region Ctor
        public CustomeOrderService(IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<Category> categoryRepository,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            IDbContext dbContext,
            IDataProvider dataProvider)
        {
            this._orderRepository = orderRepository;
            this._orderItemRepository = orderItemRepository;
            this._productCategoryRepository = productCategoryRepository;
            this._categoryRepository = categoryRepository;
            this._workContext = workContext;
            this._shoppingCartService = shoppingCartService;
            this._customerService = customerService;
            this._dbContext = dbContext;
            this._dataProvider = dataProvider;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="customerId">Customer identifier; 0 to load all orders</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="affiliateId">Affiliate identifier; 0 to load all orders</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier, only orders with products from a specified warehouse will be loaded; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="osIds">Order status identifiers; null to load all orders</param>
        /// <param name="psIds">Payment status identifiers; null to load all orders</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all orders</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">category id list</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        /// <returns>Orders</returns>
        public virtual IPagedList<Order> SearchOrders(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "",
            string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, int categoryId = 0,
            int customProductTypeId = -1)
        {
            var query = _orderRepository.Table;
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (vendorId > 0)
                query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.Product.VendorId == vendorId));
            if (customerId > 0)
                query = query.Where(o => o.CustomerId == customerId);
            if (productId > 0)
                query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.ProductId == productId));

            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem =>
                        //"Use multiple warehouses" enabled
                        //we search in each warehouse
                        (orderItem.Product.ManageInventoryMethodId == manageStockInventoryMethodId &&
                        orderItem.Product.UseMultipleWarehouses &&
                        orderItem.Product.ProductWarehouseInventory.Any(pwi => pwi.WarehouseId == warehouseId))
                        ||
                        //"Use multiple warehouses" disabled
                        //we use standard "warehouse" property
                        ((orderItem.Product.ManageInventoryMethodId != manageStockInventoryMethodId ||
                        !orderItem.Product.UseMultipleWarehouses) &&
                        orderItem.Product.WarehouseId == warehouseId)));
            }

            if (billingCountryId > 0)
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId);
            if (!string.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);
            if (affiliateId > 0)
                query = query.Where(o => o.AffiliateId == affiliateId);
            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);
            if (osIds != null && osIds.Any())
                query = query.Where(o => osIds.Contains(o.OrderStatusId));
            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));
            if (ssIds != null && ssIds.Any())
                query = query.Where(o => ssIds.Contains(o.ShippingStatusId));
            if (!string.IsNullOrEmpty(billingPhone))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.PhoneNumber) && o.BillingAddress.PhoneNumber.Contains(billingPhone));
            if (!string.IsNullOrEmpty(billingEmail))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));
            if (!string.IsNullOrEmpty(billingLastName))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName));
            if (!string.IsNullOrEmpty(orderNotes))
                query = query.Where(o => o.OrderNotes.Any(on => on.Note.Contains(orderNotes)));
            query = query.Where(o => !o.Deleted);

            if (categoryId > 0)
            {
                query = (from q in query
                         join oi in _orderItemRepository.Table on q.Id equals oi.OrderId
                         join pcm in _productCategoryRepository.Table on oi.ProductId equals pcm.ProductId
                         where pcm.CategoryId == categoryId
                         select q).Distinct();
            }
            if (customProductTypeId > -1)
            {
                query = (from q in query
                         join oi in _orderItemRepository.Table on q.Id equals oi.OrderId
                         where oi.CustomProductTypeId == customProductTypeId
                         select q).Distinct();
            }

            query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return new PagedList<Order>(query, pageIndex, pageSize, getOnlyTotalCount);
        }

        /// <summary>
        /// Get order average report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="osIds">Order status identifiers</param>
        /// <param name="psIds">Payment status identifiers</param>
        /// <param name="ssIds">Shipping status identifiers</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <param name="categoryId">category identifier; pass 0 to ignour this parameter.</param>
        /// <returns>Result</returns>
        public virtual OrderAverageReportLine GetOrderAverageReportLine(int storeId = 0,
            int vendorId = 0, int productId = 0, int billingCountryId = 0,
            int orderId = 0, string paymentMethodSystemName = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "", string orderNotes = null, int categoryId = 0,
            int customProductTypeId = -1)
        {
            var query = _orderRepository.Table;
            query = query.Where(o => !o.Deleted);
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (orderId > 0)
                query = query.Where(o => o.Id == orderId);
            if (vendorId > 0)
                query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.Product.VendorId == vendorId));
            if (productId > 0)
                query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.ProductId == productId));
            if (billingCountryId > 0)
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId);
            if (!string.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);
            if (osIds != null && osIds.Any())
                query = query.Where(o => osIds.Contains(o.OrderStatusId));
            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));
            if (ssIds != null && ssIds.Any())
                query = query.Where(o => ssIds.Contains(o.ShippingStatusId));
            if (startTimeUtc.HasValue)
                query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);
            if (endTimeUtc.HasValue)
                query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);
            if (!string.IsNullOrEmpty(billingPhone))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.PhoneNumber) && o.BillingAddress.PhoneNumber.Contains(billingPhone));
            if (!string.IsNullOrEmpty(billingEmail))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));
            if (!string.IsNullOrEmpty(billingLastName))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName));
            if (!string.IsNullOrEmpty(orderNotes))
                query = query.Where(o => o.OrderNotes.Any(on => on.Note.Contains(orderNotes)));

            if (categoryId > 0)
            {
                query = (from q in query
                         join oi in _orderItemRepository.Table on q.Id equals oi.OrderId
                         join pcm in _productCategoryRepository.Table on oi.ProductId equals pcm.ProductId
                         where pcm.CategoryId == categoryId
                         select q).Distinct();
            }

            if (customProductTypeId > -1)
            {
                query = (from q in query
                         join oi in _orderItemRepository.Table on q.Id equals oi.OrderId
                         where oi.CustomProductTypeId == customProductTypeId
                         select q).Distinct();
            }

            var item = (from oq in query
                        group oq by 1
                into result
                        select new
                        {
                            OrderCount = result.Count(),
                            OrderShippingExclTaxSum = result.Sum(o => o.OrderShippingExclTax),
                            OrderPaymentFeeExclTaxSum = result.Sum(o => o.PaymentMethodAdditionalFeeExclTax),
                            OrderTaxSum = result.Sum(o => o.OrderTax),
                            OrderTotalSum = result.Sum(o => o.OrderTotal)
                        }).Select(r => new OrderAverageReportLine
                        {
                            CountOrders = r.OrderCount,
                            SumShippingExclTax = r.OrderShippingExclTaxSum,
                            OrderPaymentFeeExclTaxSum = r.OrderPaymentFeeExclTaxSum,
                            SumTax = r.OrderTaxSum,
                            SumOrders = r.OrderTotalSum
                        }).FirstOrDefault();

            item = item ?? new OrderAverageReportLine
            {
                CountOrders = 0,
                SumShippingExclTax = decimal.Zero,
                OrderPaymentFeeExclTaxSum = decimal.Zero,
                SumTax = decimal.Zero,
                SumOrders = decimal.Zero
            };
            return item;
        }

        /// <summary>
        /// Get profit report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="osIds">Order status identifiers; null to load all records</param>
        /// <param name="psIds">Payment status identifiers; null to load all records</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all records</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <param name="categoryId">category identifier; pass 0 to ignour this parameter.</param>
        /// <returns>Result</returns>
        public virtual decimal ProfitReport(int storeId = 0, int vendorId = 0, int productId = 0,
            int billingCountryId = 0, int orderId = 0, string paymentMethodSystemName = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "", string orderNotes = null, int categoryId = 0,
            int customProductTypeId = -1)
        {
            var dontSearchPhone = string.IsNullOrEmpty(billingPhone);
            var dontSearchEmail = string.IsNullOrEmpty(billingEmail);
            var dontSearchLastName = string.IsNullOrEmpty(billingLastName);
            var dontSearchOrderNotes = string.IsNullOrEmpty(orderNotes);
            var dontSearchPaymentMethods = string.IsNullOrEmpty(paymentMethodSystemName);

            var orders = _orderRepository.Table;
            if (osIds != null && osIds.Any())
                orders = orders.Where(o => osIds.Contains(o.OrderStatusId));
            if (psIds != null && psIds.Any())
                orders = orders.Where(o => psIds.Contains(o.PaymentStatusId));
            if (ssIds != null && ssIds.Any())
                orders = orders.Where(o => ssIds.Contains(o.ShippingStatusId));

            if (categoryId > 0)
            {
                orders = (from q in orders
                          join oi in _orderItemRepository.Table on q.Id equals oi.OrderId
                          join pcm in _productCategoryRepository.Table on oi.ProductId equals pcm.ProductId
                          where pcm.CategoryId == categoryId
                          select q).Distinct();
            }

            if (customProductTypeId > -1)
            {
                orders = (from q in orders
                          join oi in _orderItemRepository.Table on q.Id equals oi.OrderId
                          where oi.CustomProductTypeId == customProductTypeId
                          select q).Distinct();
            }

            var query = from orderItem in _orderItemRepository.Table
                        join o in orders on orderItem.OrderId equals o.Id
                        where (storeId == 0 || storeId == o.StoreId) &&
                              (orderId == 0 || orderId == o.Id) &&
                              (billingCountryId == 0 || (o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId)) &&
                              (dontSearchPaymentMethods || paymentMethodSystemName == o.PaymentMethodSystemName) &&
                              (!startTimeUtc.HasValue || startTimeUtc.Value <= o.CreatedOnUtc) &&
                              (!endTimeUtc.HasValue || endTimeUtc.Value >= o.CreatedOnUtc) &&
                              !o.Deleted &&
                              (vendorId == 0 || orderItem.Product.VendorId == vendorId) &&
                              (productId == 0 || orderItem.ProductId == productId) &&
                              //we do not ignore deleted products when calculating order reports
                              //(!p.Deleted)
                              (dontSearchPhone || (o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.PhoneNumber) && o.BillingAddress.PhoneNumber.Contains(billingPhone))) &&
                              (dontSearchEmail || (o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail))) &&
                              (dontSearchLastName || (o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName))) &&
                              (dontSearchOrderNotes || o.OrderNotes.Any(oNote => oNote.Note.Contains(orderNotes)))
                        select orderItem;

            var productCost = Convert.ToDecimal(query.Sum(orderItem => (decimal?)orderItem.OriginalProductCost * orderItem.Quantity));

            var reportSummary = GetOrderAverageReportLine(
                storeId,
                vendorId,
                productId,
                billingCountryId,
                orderId,
                paymentMethodSystemName,
                osIds,
                psIds,
                ssIds,
                startTimeUtc,
                endTimeUtc,
                billingPhone,
                billingEmail,
                billingLastName,
                orderNotes,
                categoryId,
                customProductTypeId);

            var profit = reportSummary.SumOrders
                         - reportSummary.SumShippingExclTax
                         - reportSummary.OrderPaymentFeeExclTaxSum
                         - reportSummary.SumTax
                         - productCost;
            return profit;
        }

        /// <summary>
        /// Update product cart
        /// </summary>
        /// <param name="shoppingCartType"></param>
        /// <param name="product"></param>
        /// <param name="attributesXml"></param>
        /// <param name="customerEnteredPrice"></param>
        /// <param name="rentalStartDate"></param>
        /// <param name="rentalEndDate"></param>
        /// <param name="customProductTypeId"></param>
        /// <param name="storeId"></param>
        public virtual void UpdateProductTypeInCart(
            ShoppingCartType shoppingCartType,
            Product product,
            string attributesXml = "",
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null,
            DateTime? rentalEndDate = null,
            int customProductTypeId = (int)CustomProductType.New,
            string devicePackage = "",
            int storeId = 0)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == shoppingCartType)
                .LimitPerStore(storeId)
                .ToList();

            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, shoppingCartType, product, attributesXml, customerEnteredPrice, rentalStartDate, rentalEndDate);

            if (shoppingCartItem != null)
            {
                shoppingCartItem.CustomProductTypeId = customProductTypeId;
                shoppingCartItem.DevicePackage = devicePackage;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            }
        }

        /// <summary>
        /// Get Customization report 
        /// </summary>
        /// <param name="createdFromUtc"></param>
        /// <param name="createdToUtc"></param>
        /// <param name="orderStatus"></param>
        /// <param name="productId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual string SearchOrderReport(out int totalRecords,DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int orderStatus = 0, int productId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var pCreatedFromUtc = _dataProvider.GetDateTimeParameter("CreatedFromUtc", createdFromUtc);
            var pCreatedToUtc = _dataProvider.GetDateTimeParameter("CreatedToUtc", createdToUtc);
            var pOrderStatus = _dataProvider.GetInt32Parameter("OrderStatus", orderStatus);
            var pProductId = _dataProvider.GetInt32Parameter("ProductId", productId);
            var pPageIndex = _dataProvider.GetInt32Parameter("PageIndex", pageIndex);
            var pPageSize = _dataProvider.GetInt32Parameter("PageSize", pageSize);

            //prepare output parameters
            var pTotalRecords = _dataProvider.GetOutputInt32Parameter("TotalRecords");
            var jsonResult = _dataProvider.GetOutputStringParameter("JsonResult");
            jsonResult.Size = int.MaxValue - 1;
            
            _dbContext.ExecuteSqlCommand(
                "SP_CustomReport @CreatedFromUtc,@CreatedToUtc,@OrderStatus,@ProductId,@PageIndex,@PageSize,@TotalRecords OUTPUT,@JsonResult OUTPUT",
                true,
                null,
                pCreatedFromUtc, pCreatedToUtc, pOrderStatus, pProductId, pPageIndex, pPageSize, pTotalRecords, jsonResult);

            totalRecords = pTotalRecords.Value != DBNull.Value ? Convert.ToInt32(pTotalRecords.Value) : 0;

            return jsonResult.Value.ToString();
            
            //var query = _orderRepository.Table
            //    .Where(o => !o.Deleted)
            //    .Where(o => createdFromUtc.HasValue && createdFromUtc.Value <= o.CreatedOnUtc || true)
            //    .Where(o => createdToUtc.HasValue && createdToUtc.Value <= o.CreatedOnUtc || true)
            //    .Where(p => productId > 0 && p.OrderStatusId == orderStatus || true)
            //    .Where(o => productId > 0 && o.OrderItems.Any(orderItem => orderItem.ProductId == productId) || true)
            //    .Include(o => o.BillingAddress)
            //    .Include(o => o.Customer).Include(o => o.OrderItems)
            //    .ThenInclude(oi => oi.Product)
            //    .OrderByDescending(o => o.CreatedOnUtc)
            //    .ToList();

            //return new PagedList<Order>(query.ToList(), pageIndex, pageSize);

            //var query = _orderRepository.TableNoTracking;
            ////var query = _orderRepository.Table.Include(o => o.Customer).Include(o => o.BillingAddress).AsQueryable();


            //if (createdFromUtc.HasValue)
            //    query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);

            //if (createdToUtc.HasValue)
            //    query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);

            //if (orderStatus > 0)
            //    query = query.Where(p => p.OrderStatusId == orderStatus);

            //if (productId > 0)
            //    query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.ProductId == productId));

            //query = query.Where(o => !o.Deleted);

            //query = query.OrderByDescending(o => o.CreatedOnUtc);

            //if (pageSize == int.MaxValue)

            //    return new PagedList<Order>(query.ToList(), pageIndex, pageSize);

            ////database layer paging
            //return new PagedList<Order>(query, pageIndex, pageSize);
        }
        public virtual IPagedList<CrReport> SearchExportOrderReport(DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int orderStatus = 0, int productId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var sqlString = $"Exec SP_CustomReport @OrderStatus = {orderStatus},@ProductId ={productId}";
            if (createdFromUtc != null && createdToUtc != null)
                sqlString  += $",@CreatedFromUtc = '{createdFromUtc}',@CreatedToUtc = '{createdToUtc}'";
            else if (createdFromUtc != null)
                sqlString += $",@CreatedFromUtc = '{createdFromUtc}'";
            else if (createdToUtc != null)
                sqlString += $",@CreatedToUtc = '{createdToUtc}'";
            
            var crReport = _dbContext.QueryFromSql<CrReport>(sqlString).ToList();

            return new PagedList<CrReport>(crReport, pageIndex, pageSize);
            //return crReport;
        }

        #endregion
    }
}
