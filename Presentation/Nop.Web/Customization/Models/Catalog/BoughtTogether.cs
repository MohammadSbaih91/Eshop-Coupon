using Nop.Core.Domain.Enum;
using System.Collections.Generic;

namespace Nop.Web.Models.Catalog
{
    public class BoughtTogether
    {
        public BoughtTogether()
        {
            ProductOverviewModel = new List<ProductOverviewModel>();
        }

        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public EnumProductDetail enumProductDetail { get; set; }

        public IList<ProductOverviewModel> ProductOverviewModel { get; set; }
    }
}
