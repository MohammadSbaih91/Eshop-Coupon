using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;

namespace Nop.Services.Tax
{
    /// <summary>
    /// Tax service
    /// </summary>
    public partial interface ITaxService
    {

        decimal GetProductPrice(Product product, decimal price,
            int quantity, out decimal taxRate, out decimal taxRate2);

        decimal GetProductPrice(Product product, decimal price, int quantity,
            Customer customer, out decimal taxRate, out decimal taxRate2);

        decimal GetProductPrice(Product product, decimal price, int quantity,
            bool includingTax, Customer customer, out decimal taxRate, out decimal taxRate2);

        decimal GetProductPrice(Product product, int taxCategoryId, int taxCategory2Id,
            decimal price, int quantity, bool includingTax, Customer customer,
            bool priceIncludesTax, out decimal taxRate, out decimal taxRate2);
    }
}