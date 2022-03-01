using System;

namespace Nop.Core.Domain.Catalog
{
    public class SimCard : BaseEntity
    {
        public string CardNumber { get; set; }
        public int DisplayOrder { get; set; }
        public string Group { get; set; }
        public bool IsSale { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }
}
