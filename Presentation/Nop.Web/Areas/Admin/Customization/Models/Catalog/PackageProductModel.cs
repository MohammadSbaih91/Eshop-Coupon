using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial class PackageProductModel : BaseNopEntityModel, IDiscountSupportedModel
    {
        public PackageProductModel()
        {
            SelectedDiscountIds = new List<int>();
            AvailableDiscounts = new List<SelectListItem>();
        }

        public int PackageId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string DiscountIds { get; set; }
        public string DiscountName { get; set; }
        [NopResourceDisplayName("Admin.Packages.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        [NopResourceDisplayName("Admin.Packages.Fields.Discount")]
        public IList<int> SelectedDiscountIds { get; set; }
        public IList<SelectListItem> AvailableDiscounts { get; set; }
    }
}
