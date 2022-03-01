using Nop.Core.Domain.Orders;
using Nop.Services.ExportImport.Help;
using System.Collections.Generic;

namespace Nop.Services.ExportImport
{
    public partial class ExportManager : IExportManager
    {
        /// <summary>
        /// Export orders to XLSX
        /// </summary>
        /// <param name="orders">Orders</param>
        public virtual byte[] ExportCustomizeReportToXlsx(IList<Order> orders)
        {
            //a vendor should have access only to part of order information
            var ignore = _workContext.CurrentVendor != null;

            //property array
            var properties = new[]
            {
                new PropertyByName<Order>("OrderId", p => p.Id),
                new PropertyByName<Order>("StoreId", p => p.StoreId),
                new PropertyByName<Order>("OrderGuid", p => p.OrderGuid, ignore),
                new PropertyByName<Order>("CustomerId", p => p.CustomerId, ignore),
                new PropertyByName<Order>("OrderStatusId", p => p.OrderStatusId, ignore),
                new PropertyByName<Order>("PaymentStatusId", p => p.PaymentStatusId),
                new PropertyByName<Order>("ShippingStatusId", p => p.ShippingStatusId, ignore),
                new PropertyByName<Order>("OrderSubtotalInclTax", p => p.OrderSubtotalInclTax, ignore),
                new PropertyByName<Order>("OrderSubtotalExclTax", p => p.OrderSubtotalExclTax, ignore),
                new PropertyByName<Order>("OrderSubTotalDiscountInclTax", p => p.OrderSubTotalDiscountInclTax, ignore),
                new PropertyByName<Order>("OrderSubTotalDiscountExclTax", p => p.OrderSubTotalDiscountExclTax, ignore),
                new PropertyByName<Order>("OrderShippingInclTax", p => p.OrderShippingInclTax, ignore),
                new PropertyByName<Order>("OrderShippingExclTax", p => p.OrderShippingExclTax, ignore),
                new PropertyByName<Order>("PaymentMethodAdditionalFeeInclTax", p => p.PaymentMethodAdditionalFeeInclTax, ignore),
                new PropertyByName<Order>("PaymentMethodAdditionalFeeExclTax", p => p.PaymentMethodAdditionalFeeExclTax, ignore),
                new PropertyByName<Order>("TaxRates", p => p.TaxRates, ignore),
                new PropertyByName<Order>("OrderTax", p => p.OrderTax, ignore),
                new PropertyByName<Order>("OrderTotal", p => p.OrderTotal, ignore),
                new PropertyByName<Order>("RefundedAmount", p => p.RefundedAmount, ignore),
                new PropertyByName<Order>("OrderDiscount", p => p.OrderDiscount, ignore),
                new PropertyByName<Order>("CurrencyRate", p => p.CurrencyRate),
                new PropertyByName<Order>("CustomerCurrencyCode", p => p.CustomerCurrencyCode),
                new PropertyByName<Order>("AffiliateId", p => p.AffiliateId, ignore),
                new PropertyByName<Order>("PaymentMethodSystemName", p => p.PaymentMethodSystemName, ignore),
                new PropertyByName<Order>("ShippingPickUpInStore", p => p.PickUpInStore, ignore),
                new PropertyByName<Order>("ShippingMethod", p => p.ShippingMethod),
                new PropertyByName<Order>("ShippingRateComputationMethodSystemName", p => p.ShippingRateComputationMethodSystemName, ignore),
                new PropertyByName<Order>("CustomValuesXml", p => p.CustomValuesXml, ignore),
                new PropertyByName<Order>("VatNumber", p => p.VatNumber, ignore),
                new PropertyByName<Order>("CreatedOnUtc", p => p.CreatedOnUtc.ToOADate()),
                new PropertyByName<Order>("BillingFirstName", p => p.BillingAddress?.FirstName ?? string.Empty),
                new PropertyByName<Order>("BillingLastName", p => p.BillingAddress?.LastName ?? string.Empty),
                new PropertyByName<Order>("BillingEmail", p => p.BillingAddress?.Email ?? string.Empty),
                new PropertyByName<Order>("BillingCompany", p => p.BillingAddress?.Company ?? string.Empty),
                new PropertyByName<Order>("BillingCountry", p => p.BillingAddress?.Country?.Name ?? string.Empty),
                new PropertyByName<Order>("BillingStateProvince", p => p.BillingAddress?.StateProvince?.Name ?? string.Empty),
                new PropertyByName<Order>("BillingCounty", p => p.BillingAddress?.County ?? string.Empty),
                new PropertyByName<Order>("BillingCity", p => p.BillingAddress?.City ?? string.Empty),
                new PropertyByName<Order>("BillingAddress1", p => p.BillingAddress?.Address1 ?? string.Empty),
                new PropertyByName<Order>("BillingAddress2", p => p.BillingAddress?.Address2 ?? string.Empty),
                new PropertyByName<Order>("BillingZipPostalCode", p => p.BillingAddress?.ZipPostalCode ?? string.Empty),
                new PropertyByName<Order>("BillingPhoneNumber", p => p.BillingAddress?.PhoneNumber ?? string.Empty),
                new PropertyByName<Order>("BillingFaxNumber", p => p.BillingAddress?.FaxNumber ?? string.Empty),
                new PropertyByName<Order>("ShippingFirstName", p => p.ShippingAddress?.FirstName ?? string.Empty),
                new PropertyByName<Order>("ShippingLastName", p => p.ShippingAddress?.LastName ?? string.Empty),
                new PropertyByName<Order>("ShippingEmail", p => p.ShippingAddress?.Email ?? string.Empty),
                new PropertyByName<Order>("ShippingCompany", p => p.ShippingAddress?.Company ?? string.Empty),
                new PropertyByName<Order>("ShippingCountry", p => p.ShippingAddress?.Country?.Name ?? string.Empty),
                new PropertyByName<Order>("ShippingStateProvince", p => p.ShippingAddress?.StateProvince?.Name ?? string.Empty),
                new PropertyByName<Order>("ShippingCounty", p => p.ShippingAddress?.County ?? string.Empty),
                new PropertyByName<Order>("ShippingCity", p => p.ShippingAddress?.City ?? string.Empty),
                new PropertyByName<Order>("ShippingAddress1", p => p.ShippingAddress?.Address1 ?? string.Empty),
                new PropertyByName<Order>("ShippingAddress2", p => p.ShippingAddress?.Address2 ?? string.Empty),
                new PropertyByName<Order>("ShippingZipPostalCode", p => p.ShippingAddress?.ZipPostalCode ?? string.Empty),
                new PropertyByName<Order>("ShippingPhoneNumber", p => p.ShippingAddress?.PhoneNumber ?? string.Empty),
                new PropertyByName<Order>("ShippingFaxNumber", p => p.ShippingAddress?.FaxNumber ?? string.Empty)
            };

            return new PropertyManager<Order>(properties, _catalogSettings).ExportToXlsx(orders);
        }
    }
}
