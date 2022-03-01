using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Shipping.Aramex.Services
{
    public class TrackingService : ITrackingService
    {
        #region Fields
        private readonly IShipmentService _shipmentService;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor
        public TrackingService(IShipmentService shipmentService,
            IOrderService orderService,
            ILocalizationService localizationService)
        {
            _shipmentService = shipmentService;
            _orderService = orderService;
            _localizationService = localizationService;
        }
        #endregion

        #region Methods
        
        public string GetOrderTrackingNumber(int orderId,string emailId, out string errorMessage)
        {
            var trackingNumber = "";
            errorMessage = "";

            var order = _orderService.GetOrderById(orderId);
            if (order != null)
            {
                if (order.Customer.Email == emailId)
                {
                    if (order.Shipments != null && order.Shipments.Count > 0)
                    {
                        var shipment = order.Shipments.FirstOrDefault();
                        if (!string.IsNullOrEmpty(shipment.TrackingNumber))
                            trackingNumber = shipment.TrackingNumber;
                        else
                            errorMessage = _localizationService.GetResource("Shipping.Aramex.Tracking.ShipmentNotCreated");
                    }
                    else
                        errorMessage = _localizationService.GetResource("Shipping.Aramex.Tracking.ShipmentNotCreated");
                }
                else
                    errorMessage = _localizationService.GetResource("Shipping.Aramex.Tracking.OrderNotMatchWithEmail");
            }
            errorMessage = _localizationService.GetResource("Shipping.Aramex.Tracking.OrderNotFind");
            return trackingNumber;
        }
        #endregion
    }
}
