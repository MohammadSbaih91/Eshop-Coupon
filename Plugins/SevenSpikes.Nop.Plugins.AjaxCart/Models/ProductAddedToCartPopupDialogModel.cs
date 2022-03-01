
using Nop.Web.Models.ShoppingCart;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Models
{
  public class ProductAddedToCartPopupDialogModel
  {
    public ProductAddedToCartPopupDialogModel()
    {
      MiniShoppingCart = new MiniShoppingCartModel();
      EnableRelatedProductsInPopup = false;
      EnableCrossSellProductsInPopup = false;
    }

    public MiniShoppingCartModel MiniShoppingCart { get; set; }

    public bool EnableRelatedProductsInPopup { get; set; }

    public bool EnableCrossSellProductsInPopup { get; set; }
  }
}
