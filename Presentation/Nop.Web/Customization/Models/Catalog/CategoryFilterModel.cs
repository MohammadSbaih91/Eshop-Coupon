using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Nop.Web.Customization.Models.Catalog
{
    public class CategoryFilterModel
    {
        public CategoryFilterModel()
        {
            Category = new List<SelectListItem>();
            SubCategory = new List<SelectListItem>();
        }

        public IList<SelectListItem> Category { get; set; }
        public IList<SelectListItem> SubCategory { get; set; }
    }
}
