using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Customization.Models.Catalog
{
    public class HomePageProduct : BaseNopModel
    {
        public HomePageProduct()
        {
            Products = new List<ProductOverviewModel>();
            categoryModel = new CategoryModel();
        }

        public int CurrentPage { get; set; }
        public int TotalPage { get; set; }
        public CategoryModel categoryModel { get; set; }

        public string CategoryProductBoxTemplate { get; set; }

        public int ActiveCategoryId { get; set; }

        public string OwlCarouselDivId { get; set; }

        public IList<ProductOverviewModel> Products { get; set; }

        public string ParentCategoryName { get; set; }

        public bool IsDrawer { get; set; }
    }
}
