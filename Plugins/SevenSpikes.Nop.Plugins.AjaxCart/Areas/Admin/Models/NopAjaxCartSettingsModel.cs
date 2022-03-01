
using Nop.Web.Framework.Mvc.ModelBinding;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Areas.Admin.Models
{
  public class NopAjaxCartSettingsModel
  {
    public bool IsTrialVersion { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.EnableOnMobileDevices")]
    public bool EnableOnMobileDevices { get; set; }

    public bool EnableOnMobileDevices_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.EnableNopAjaxCart")]
    public bool EnableNopAjaxCart { get; set; }

    public bool EnableNopAjaxCart_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.EnableCartWishlistOnProductPage")]
    public bool EnableOnProductPage { get; set; }

    public bool EnableOnProductPage_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.EnableCartWishlistOnCatalogPages")]
    public bool EnableOnCatalogPages { get; set; }

    public bool EnableOnCatalogPages_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.EnableProductQuantityTextBox")]
    public bool EnableProductQuantityTextBox { get; set; }

    public bool EnableProductQuantityTextBox_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.WishlistMenuLinkSelector")]
    public string WishlistMenuLinkSelector { get; set; }

    public bool WishlistMenuLinkSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.AddToWishlistButtonSelector")]
    public string AddToWishlistButtonSelector { get; set; }

    public bool AddToWishlistButtonSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.ShoppingCartMenuLinkSelector")]
    public string ShoppingCartMenuLinkSelector { get; set; }

    public bool ShoppingCartMenuLinkSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.FlyoutCartPanelSelector")]
    public string FlyoutCartPanelSelector { get; set; }

    public bool FlyoutCartPanelSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.ProductPageAddToCartButtonSelector")]
    public string ProductPageAddToCartButtonSelector { get; set; }

    public bool ProductPageAddToCartButtonSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.ProductBoxAddToCartButtonSelector")]
    public string ProductBoxAddToCartButtonSelector { get; set; }

    public bool ProductBoxAddToCartButtonSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.ProductBoxProductItemElementSelector")]
    public string ProductBoxProductItemElementSelector { get; set; }

    public bool ProductBoxProductItemElementSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.ProductAddedToCartImageSizeInPixels")]
    public int ProductAddedToCartImageSize { get; set; }

    public bool ProductAddedToCartImageSize_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.EnableRelatedProductsInPopup")]
    public bool EnableRelatedProductsInPopup { get; set; }

    public bool EnableRelatedProductsInPopup_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.NumberOfRelatedProductsInPopup")]
    public int NumberOfRelatedProductsInPopup { get; set; }

    public bool NumberOfRelatedProductsInPopup_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.EnableCrossSellProductsInPopup")]
    public bool EnableCrossSellProductsInPopup { get; set; }

    public bool EnableCrossSellProductsInPopup_OverrideForStore { get; set; }

    [NopResourceDisplayName("SevenSpikes.NopAjaxCart.Admin.Settings.NumberOfCrossSellProductsInPopup")]
    public int NumberOfCrossSellProductsInPopup { get; set; }

    public bool NumberOfCrossSellProductsInPopup_OverrideForStore { get; set; }

    public string Theme { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
  }
}
