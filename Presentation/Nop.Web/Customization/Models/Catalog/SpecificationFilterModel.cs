using System.Collections.Generic;

namespace Nop.Web.Models.Catalog
{
    public partial class CatalogPagingFilteringModel
    {

        public partial class SpecificationFilterModel
        {
            public List<SpecificationFilterItem> AllFilteredItems = new List<SpecificationFilterItem>();
        }

        public partial class SpecificationFilterItem
        {
            public bool IsAlreadyFiltered { get; set; }
            public bool IsDisabled { get; set; }
        }
    }

}
