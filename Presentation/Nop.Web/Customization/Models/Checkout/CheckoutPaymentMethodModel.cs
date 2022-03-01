using static Nop.Web.Models.ShoppingCart.ShoppingCartModel;

namespace Nop.Web.Models.Checkout
{
    public partial class CheckoutPaymentMethodModel
    {
        public DiscountBoxModel DiscountBox { get; set; } = new DiscountBoxModel();
        public GiftCardBoxModel GiftCardBox { get; set; } = new GiftCardBoxModel();
    }
}
