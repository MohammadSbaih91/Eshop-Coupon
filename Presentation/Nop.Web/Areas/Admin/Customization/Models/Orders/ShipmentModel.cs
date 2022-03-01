using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    public partial class ShipmentModel
    {
        [NopResourceDisplayName("Admin.Orders.Shipments.ExpectedDeliveryDate")]
        [UIHint("DateNullable")]
        public DateTime? ExpectedDeliveryDate { get; set; }
    }
}
