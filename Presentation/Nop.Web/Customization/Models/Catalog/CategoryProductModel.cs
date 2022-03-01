using Nop.Web.Models.Catalog;
using System.Collections.Generic;
using static Nop.Web.Models.Catalog.CategoryModel;

namespace Nop.Web.Customization.Models.Catalog
{
    public class CategoryProductModel
    {
        public CategoryProductModel()
        {
            ProductOverviewModel = new List<ProductOverviewModel>();
            SubCategories = new List<SubCategoryModel>();
        }

        public int CategoryId { get; set; }
        public IList<SubCategoryModel> SubCategories { get; set; }
        public string CategoryProductBoxTemplate { get; set; }
        public IList<ProductOverviewModel> ProductOverviewModel { get; set; }
    }
}
