using Nop.Core;
using System.Collections.Generic;


namespace Nop.Web.Models.Catalog
{
    public class PackageModel : BaseEntity
    {
        public PackageModel()
        {
            Products = new List<ProductOverviewModel>();
        }
        public string PackageName { get; set; }

        public IList<ProductOverviewModel> Products { get; set; }
    }
}
