using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Customization.Models.Report
{
    public class CustomReportSearchModel : BaseSearchModel
    {
        public CustomReportSearchModel()
        {
            AvailableOrderStatuses = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Reports.CustomReport.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.Reports.CustomReport.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.Reports.CustomReport.OrderStatus")]
        public int OrderStatusId { get; set; }
        public IList<SelectListItem> AvailableOrderStatuses { get; set; }

        [NopResourceDisplayName("Admin.Reports.CustomReport.ProductName")]
        public int ProductId { get; set; }
        
    }
}
