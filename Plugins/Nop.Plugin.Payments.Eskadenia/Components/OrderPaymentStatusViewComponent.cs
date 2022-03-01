using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Eskadenia.Models;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Checkout;

namespace Nop.Plugin.Payments.Eskadenia.Components
{
    [ViewComponent(Name = "OrderPaymentStatus")]
    public class OrderPaymentStatusViewComponent : NopViewComponent
    {
        #region fields

        private readonly IOrderService _orderSercvice;
        private readonly IPaymentService _paymentService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region ctor

        public OrderPaymentStatusViewComponent(IOrderService orderSercvice,
            IPaymentService paymentService,
            ILocalizationService localizationService)
        {
            this._orderSercvice = orderSercvice;
            this._paymentService = paymentService;
            this._localizationService = localizationService;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var checkoutCompletedModel = (CheckoutCompletedModel)additionalData;

            var order = _orderSercvice.GetOrderById(checkoutCompletedModel.OrderId);
            if (order == null)
                return Content("");

            if (order.PaymentMethodSystemName != "Payments.Eskadenia")
                return Content("");

            PaymentStatusModel model = new PaymentStatusModel()
            {
                OrderId = order.Id
            };

            if (order.PaymentStatus == Core.Domain.Payments.PaymentStatus.Paid)
            {
                model.IsPaid = true;
            }
            else
            {
                model.IsPaid = false;
                model.CanRePostProcessPayment = _paymentService.CanRePostProcessPayment(order);
                if (!string.IsNullOrWhiteSpace(order.AuthorizationTransactionCode))
                    model.FailStatusMessage = string.Format(_localizationService.GetResource("Plugins.Payments.Eskadenia.Payment.Fail.StatusCode"), order.AuthorizationTransactionCode);
            }

            return View(model);
        }
    }
}
