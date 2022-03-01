using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;

namespace Nop.Web.Areas.Admin.Customization.Models.Report
{
    public class CustomReportModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.Report.Customization.OrderId")]
        public string CustomOrderNumber { get; set; }

        //order status
        [NopResourceDisplayName("Admin.Report.Customization.OrderStatus")]
        public string OrderStatus { get; set; }
        public int OrderStatusId { get; set; }

        [NopResourceDisplayName("Admin.Report.Customization.OrderProduct")]
        public string OrderProduct { get; set; }

        [NopResourceDisplayName("Admin.Report.Customization.Vender")]
        public string Vender { get; set; }

        [NopResourceDisplayName("Admin.Report.Customization.CustomerName")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Admin.Report.Customization.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Admin.Report.Customization.CustomerAddress")]
        public string CustomerAddress { get; set; }

        [NopResourceDisplayName("Admin.Report.Customization.CustomerEmail")]
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.Report.Customization.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.Report.Customization.OrderTotal")]
        public string OrderTotal { get; set; }

        [NopResourceDisplayName("Admin.Report.Customization.CustomerId")]
        public string CustomerId { get; set; }
    }
}
