using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.eShopSMS.Services;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Models.Orders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.eShopSMS.Areas.Admin.Filters
{
    public class CustomActionFilter : ActionFilterAttribute
    {
        #region Fields
        private readonly IOrderService _orderService;
        private readonly ISendSMSNotificationService _sendSMSNotificationService;
        #endregion

        #region Ctor
        public CustomActionFilter(IOrderService orderService,
            ISendSMSNotificationService sendSMSNotificationService)
        {
            _orderService = orderService;
            _sendSMSNotificationService = sendSMSNotificationService;
        }
        #endregion

        #region Methods
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(OrderController) &&
                actionDescriptor.ActionName == "Edit" && actionDescriptor.MethodInfo.Name == "ChangeOrderStatus" &&
                context.HttpContext.Request.Method == "POST")
            {
                var model = (OrderModel)context.ActionArguments["model"];
                var orderId = (int)context.ActionArguments["id"];

                var order = _orderService.GetOrderById(orderId);

                if (model.OrderStatusId == (int)OrderStatus.Cancelled)
                    _sendSMSNotificationService.SendCustomerNotification(SMSTemplateName.OrderCancelled, order);

                if (model.OrderStatusId == (int)OrderStatus.Complete)
                    _sendSMSNotificationService.SendCustomerNotification(SMSTemplateName.OrderCompleted, order);

                if (model.OrderStatusId == (int)OrderStatus.Uncovered || model.OrderStatusId == (int)OrderStatus.Unreachable)
                    _sendSMSNotificationService.SendCustomerNotification(SMSTemplateName.OrderUncovered, order);

                //if (model.OrderStatusId == )
                //    _sendSMSNotificationService.SendCustomerNotification(SMSTemplateName.ServiceUncovered, order);

            }

            if (actionDescriptor.ControllerTypeInfo == typeof(OrderController) &&
                actionDescriptor.ActionName == "Edit" && actionDescriptor.MethodInfo.Name == "CancelOrderWithReason" &&
                context.HttpContext.Request.Method == "POST")
            {
                var orderId = (int)context.ActionArguments["id"];

                var order = _orderService.GetOrderById(orderId);
                
                _sendSMSNotificationService.SendCustomerNotification(SMSTemplateName.OrderCancelled, order);
            }
        }
        #endregion
    }
}
