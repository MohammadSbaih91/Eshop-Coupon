
using Nop.Core.Configuration;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Domain
{
  public class NopAjaxCartSettings : ISettings
  {
    public bool EnableNopAjaxCart { get; set; }

    public bool EnableOnMobileDevices { get; set; }

    public bool EnableOnProductPage { get; set; }

    public bool EnableOnCatalogPages { get; set; }

    public bool EnableProductQuantityTextBox { get; set; }

    public string WishlistMenuLinkSelector { get; set; }

    public string AddToWishlistButtonSelector { get; set; }

    public string ShoppingCartMenuLinkSelector { get; set; }

    public string FlyoutCartPanelSelector { get; set; }

    public string ProductPageAddToCartButtonSelector { get; set; }

    public string ProductBoxAddToCartButtonSelector { get; set; }

    public string ProductBoxProductItemElementSelector { get; set; }

    public int ProductAddedToCartImageSize { get; set; }

    public bool EnableRelatedProductsInPopup { get; set; }

    public int NumberOfRelatedProductsInPopup { get; set; }

    public bool EnableCrossSellProductsInPopup { get; set; }

    public int NumberOfCrossSellProductsInPopup { get; set; }
  }
}
