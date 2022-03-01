using Nop.Core.Domain.Catalog;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nop.Core.Domain.Orders
{
    public partial class ShoppingCartItem
    {
        /// <summary>
        /// get or set CustomProductTypeId
        /// </summary>
        public int CustomProductTypeId { get; set; }

        public int SimCardId { get; set; }

        public string DevicePackage { get; set; }

        public int PackageId { get; set; }
        public decimal SubsidyDiscount { get; set; }

        /// <summary>
        /// Gets the CustomProductType
        /// </summary>
        [NotMapped]
        public CustomProductType CustomProductType
        {
            get => (CustomProductType)CustomProductTypeId;
            set => CustomProductTypeId = (int)value;
        }
    }
}
