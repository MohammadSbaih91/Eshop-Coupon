using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial class CategoryModel
    {
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.CategoryProductBoxTemplate")]
        public int CategoryProductBoxTemplateId { get; set; }
        public IList<SelectListItem> AvailableCategoryProductBoxTemplates { get; set; } = new List<SelectListItem>();

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.ShowWithSubCategories")]
        public bool ShowWithSubCategories { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MinimumPriceOfProduct")]
        public decimal MinimumPriceOfProduct { get; set; }
    }
}
