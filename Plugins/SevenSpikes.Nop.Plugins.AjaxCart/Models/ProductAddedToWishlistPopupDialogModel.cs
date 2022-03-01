
using Nop.Web.Models.ShoppingCart;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Models
{
  public class ProductAddedToWishlistPopupDialogModel
  {
    public ProductAddedToWishlistPopupDialogModel()
    {
      WishlistShoppingCartItemModel = new WishlistModel.ShoppingCartItemModel();
      EnableRelatedProductsInPopup = false;
      EnableCrossSellProductsInPopup = false;
    }

    public WishlistModel.ShoppingCartItemModel WishlistShoppingCartItemModel { get; set; }

    public bool EnableRelatedProductsInPopup { get; set; }

    public bool EnableCrossSellProductsInPopup { get; set; }
  }
}
