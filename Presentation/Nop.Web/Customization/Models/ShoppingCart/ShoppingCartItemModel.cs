using Nop.Web.Framework.Models;

namespace Nop.Web.Models.ShoppingCart
{
    public partial class ShoppingCartModel : BaseNopModel
    {
        public partial class ShoppingCartItemModel
        {
            public TaxSplitInfoModel TaxSplitInfo { get; set; } = new TaxSplitInfoModel();

            public decimal SubTotalValue { get; set; }

            public int SimCardId { get; set; }

            public string SimCardNumber { get; set; }

            public bool IsServiceProductAddedToCart { get; set; }

            public int PackageId { get; set; }
            public decimal SubsidyDiscount { get; set; }
        }

    }

    public partial class WishlistModel
    {
        public partial class ShoppingCartItemModel
        {
            public TaxSplitInfoModel TaxSplitInfo { get; set; } = new TaxSplitInfoModel();
        }
    }

    public partial class MiniShoppingCartModel
    {
        public partial class ShoppingCartItemModel
        {
            public TaxSplitInfoModel TaxSplitInfo { get; set; } = new TaxSplitInfoModel();
        }
    }
}