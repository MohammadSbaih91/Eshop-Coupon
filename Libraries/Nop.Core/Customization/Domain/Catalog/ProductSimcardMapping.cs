using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Catalog
{
    public class ProductSimcardMapping : BaseEntity
    {
        public int ProductId { get; set; }
        public int SimCardId { get; set; }
    }
}
