using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    public partial class OrderSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Orders.List.SearchCategory")]
        public int SearchCategoryId { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();

        [NopResourceDisplayName("Admin.Orders.List.SearchCustomProductType")]
        public int CustomProductTypeId { get; set; } = -1;
        public IList<SelectListItem> AvailableCustomProductType { get; set; } = new List<SelectListItem>();
        #endregion
    }
}
