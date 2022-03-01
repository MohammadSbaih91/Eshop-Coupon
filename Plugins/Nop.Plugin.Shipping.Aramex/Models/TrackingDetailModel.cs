using Nop.Web.Framework.Models;
using Nop.Plugin.Shipping.Aramex.Models.Responce;
using Nop.Core.Domain.Orders;
using System;
using Nop.Web.Models.Order;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Aramex.Models
{
    public class TrackingDetailModel : BaseNopModel
    {
        public TrackingDetailModel()
        {
            ShipmentTrackingResponse = new ShipmentTrackingResponse();
            TrackingOrderItemModels = new List<TrackingOrderItemModel>();
            Order = new Order();
        }

        public ShipmentTrackingResponse ShipmentTrackingResponse { get; set; }
        public Order Order { get; set; }
        public IList<TrackingOrderItemModel> TrackingOrderItemModels { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime OrderDate { get; set; }
        public int TotalQty { get; set; }
        public string TotalTax { get; set; }
        public string TotalPrice { get; set; }
    }

    public class TrackingOrderItemModel
    {
        public string ImageUrl { get; set; }
        public string ProductName { get; set; }
        public string ShortDescription { get; set; }
        public int Qty { get; set; }
        public string Tax { get; set; }
        public decimal TaxValue { get; set; }
        public string Price { get; set; }
        public Decimal PriceValue { get; set; }
        public int PackageId { get; set; }
        public string AttributeInfo { get; set; }
    }
}
