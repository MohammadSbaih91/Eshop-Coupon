using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.Catalog
{
    public partial class CategoryModel
    {
        public int ActiveCategoryId { get; set; }

        public string OwlCarouselDivId { get; set; }

        public partial class SubCategoryModel
        {
            public int ParentCategoryId { get; set; }
            public decimal MinimumPriceOfProduct { get; set; }
            public string FormatMinimumPriceOfProduct { get; set; }

            public List<SubCategoryModel> ChildCategoryModels { get; set; } = new List<SubCategoryModel>();
        }
    }
}
