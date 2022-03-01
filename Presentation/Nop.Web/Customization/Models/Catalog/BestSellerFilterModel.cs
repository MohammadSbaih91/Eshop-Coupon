using System.Collections.Generic;

namespace Nop.Web.Customization.Models.Catalog
{
    public class BestSellerFilterModel
    {
        public BestSellerFilterModel()
        {
            FilterTagModel = new List<FilterTagModel>();
        }

        public IList<FilterTagModel> FilterTagModel { get; set; }

        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }

        public bool IsDrawer { get; set; }

    }

    public class FilterTagModel
    {
        public string ProductTagName { get; set; }
        public int ProductTagId { get; set; }

        public string URL { get; set; }

        
        public bool IsSelected { get; set; }
    }
}
