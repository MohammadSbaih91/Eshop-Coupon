using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Catalog
{
    public class PackageProduct : BaseEntity
    {
        public int PackageId { get; set; }
        public int ProductId { get; set; }
        public string DiscountIds { get; set; }
        public int DisplayOrder { get; set; }
    }
}
