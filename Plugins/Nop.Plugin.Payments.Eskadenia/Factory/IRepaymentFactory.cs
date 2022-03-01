using Nop.Core.Domain.Orders;
using Nop.Web.Models.Checkout;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.Eskadenia.Factory
{
    public partial interface IRepaymentFactory
    {

        /// <summary>
        /// Prepare payment method model
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="filterByCountryId">Filter by country identifier</param>
        /// <returns>Payment method model</returns>
        CheckoutPaymentMethodModel PreparePaymentMethodModel(IList<ShoppingCartItem> cart, int filterByCountryId);
    }
}
