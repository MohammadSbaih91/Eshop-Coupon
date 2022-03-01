using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Services.Customization.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class OrderController
    {
        private readonly IOrderProcessingServiceOverride _orderProcessingServiceOverride = EngineContext.Current.Resolve<IOrderProcessingServiceOverride>();

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderStatus")]
        public virtual IActionResult ChangeOrderStatus(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //Comment this code because cr (vendor can able to change order status)
            ////a vendor does not have access to this functionality
            //if (_workContext.CurrentVendor != null)
            //    return RedirectToAction("Edit", "Order", new { id });

            try
            {
                order.OrderStatusId = model.OrderStatusId;
                _orderService.UpdateOrder(order);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = $"Order status has been edited. New status: {_localizationService.GetLocalizedEnum(order.OrderStatus)}",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });


                if (order.OrderStatus == OrderStatus.Complete)
                {
                    //notification
                    var orderCompletedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderCompletedEmail ?
                        _pdfService.PrintOrderToPdf(order) : null;
                    var orderCompletedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderCompletedEmail ?
                        "order.pdf" : null;
                    var orderCompletedCustomerNotificationQueuedEmailIds = _workflowMessageService
                        .SendOrderCompletedCustomerNotification(order, order.CustomerLanguageId, orderCompletedAttachmentFilePath,
                        orderCompletedAttachmentFileName);
                }
                if (order.OrderStatus == OrderStatus.Cancelled)
                {
                    //notification
                    var orderCancelledCustomerNotificationQueuedEmailIds = _workflowMessageService.SendOrderCancelledCustomerNotification(order, order.CustomerLanguageId);
                }

                if (order.OrderStatus == OrderStatus.Uncovered)
                {
                    //notification
                    var orderUncoveredCustomerNotificationQueuedEmailIds = _workflowMessageService.SendOrderUncoveredCustomerNotification(order, order.CustomerLanguageId);
                }

                if (order.OrderStatus == OrderStatus.Unreachable)
                {
                    //notification
                    var orderUnreachableCustomerNotificationQueuedEmailIds = _workflowMessageService.SendOrderUnreachableCustomerNotification(order, order.CustomerLanguageId);
                }

                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);
                
                //prepare model
                model = _orderModelFactory.PrepareOrderModel(model, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                model = _orderModelFactory.PrepareOrderModel(model, order);

                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelorderwithreason")]
        public virtual IActionResult CancelOrderWithReason(int id,string cancelreason)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //Comment this code because cr (vendor can able to change order status)
            ////a vendor does not have access to this functionality
            //if (_workContext.CurrentVendor != null)
            //    return RedirectToAction("Edit", "Order", new { id });

            try
            {
                _orderProcessingServiceOverride.CancelOrderWithReason(order, true, cancelreason);
                
                _orderService.UpdateOrder(order);

                LogEditOrder(order.Id);

                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult AddShipment(int orderId, IFormCollection form, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToOrder(order))
                return RedirectToAction("List");

            var orderItems = order.OrderItems;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                orderItems = orderItems.Where(HasAccessToOrderItem).ToList();
            }

            Shipment shipment = null;
            decimal? totalWeight = null;
            foreach (var orderItem in orderItems)
            {
                //is shippable
                if (!orderItem.Product.IsShipEnabled)
                    continue;

                //ensure that this product can be shipped (have at least one item to ship)
                var maxQtyToAdd = _orderService.GetTotalNumberOfItemsCanBeAddedToShipment(orderItem);
                if (maxQtyToAdd <= 0)
                    continue;

                var qtyToAdd = 0; //parse quantity
                foreach (var formKey in form.Keys)
                    if (formKey.Equals($"qtyToAdd{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out qtyToAdd);
                        break;
                    }

                var warehouseId = 0;
                if (orderItem.Product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    orderItem.Product.UseMultipleWarehouses)
                {
                    //multiple warehouses supported
                    //warehouse is chosen by a store owner
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"warehouse_{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int.TryParse(form[formKey], out warehouseId);
                            break;
                        }
                }
                else
                {
                    //multiple warehouses are not supported
                    warehouseId = orderItem.Product.WarehouseId;
                }

                foreach (var formKey in form.Keys)
                    if (formKey.Equals($"qtyToAdd{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out qtyToAdd);
                        break;
                    }

                //validate quantity
                if (qtyToAdd <= 0)
                    continue;
                if (qtyToAdd > maxQtyToAdd)
                    qtyToAdd = maxQtyToAdd;

                //ok. we have at least one item. let's create a shipment (if it does not exist)

                var orderItemTotalWeight = orderItem.ItemWeight * qtyToAdd;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }

                if (shipment == null)
                {
                    var trackingNumber = form["TrackingNumber"];
                    var adminComment = form["AdminComment"];
                    var expectedDeliveryDate = form["ExpectedDeliveryDate"];
                    shipment = new Shipment
                    {
                        OrderId = order.Id,
                        TrackingNumber = trackingNumber,
                        TotalWeight = null,
                        ShippedDateUtc = null,
                        DeliveryDateUtc = null,
                        AdminComment = adminComment,
                        CreatedOnUtc = DateTime.UtcNow,
                        ExpectedDeliveryDate = null
                    };
                    if (!string.IsNullOrEmpty(expectedDeliveryDate))
                        shipment.ExpectedDeliveryDate = Convert.ToDateTime(expectedDeliveryDate);
                }

                //create a shipment item
                var shipmentItem = new ShipmentItem
                {
                    OrderItemId = orderItem.Id,
                    Quantity = qtyToAdd,
                    WarehouseId = warehouseId
                };
                shipment.ShipmentItems.Add(shipmentItem);
            }

            //if we have at least one item in the shipment, then save it
            if (shipment != null && shipment.ShipmentItems.Any())
            {
                shipment.TotalWeight = totalWeight;
                _shipmentService.InsertShipment(shipment);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "A shipment has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);

                SuccessNotification(_localizationService.GetResource("Admin.Orders.Shipments.Added"));
                return continueEditing
                           ? RedirectToAction("ShipmentDetails", new { id = shipment.Id })
                           : RedirectToAction("Edit", new { id = orderId });
            }

            ErrorNotification(_localizationService.GetResource("Admin.Orders.Shipments.NoProductsSelected"));

            return RedirectToAction("AddShipment", new { orderId });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setexpecteddeliverydate")]
        public virtual IActionResult SetExpectedDeliveryDate(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            shipment.ExpectedDeliveryDate = model.ExpectedDeliveryDate;
            _shipmentService.UpdateShipment(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        #region Order notes
        [HttpPost]
        public virtual IActionResult OrderNotesSelect(OrderNoteSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(searchModel.OrderId)
                ?? throw new ArgumentException("No order found with the specified id");

            //a vendor does not have access to this functionality
            //if (_workContext.CurrentVendor != null)
            //    return Content(string.Empty);

            //prepare model
            var model = _orderModelFactory.PrepareOrderNoteListModel(searchModel, order);

            return Json(model);
        }
        #endregion
    }
}
