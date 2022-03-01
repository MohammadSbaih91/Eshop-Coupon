
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Plugins.AjaxCart.Domain;
using SevenSpikes.Nop.Plugins.AjaxCart.Models;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Components
{
  [ViewComponent(Name = "NopAjaxCartMiniProductDetailsViewAddToCartButton")]
  public class NopAjaxCartMiniProductDetailsViewAddToCartButtonComponent : ViewComponent
  {
    private readonly ILocalizationService _localizationService;

    public NopAjaxCartMiniProductDetailsViewAddToCartButtonComponent(
      NopAjaxCartSettings nopAjaxCartSettings,
      ILocalizationService localizationService,
      IProductService productService,
      IStoreContext storeContext,
      IPermissionService permissionService,
      IEncryptionService encryptionService,
      IWebHelper webHelper)
    {
      _localizationService = localizationService;
    }

    public IViewComponentResult Invoke(
      ProductDetailsModel.AddToCartModel addToCartModel)
    {
      var cartAjaxButtonModel = new AddProductToCartAjaxButtonModel
      {
        ProductId = addToCartModel.ProductId,
        ButtonValue = !addToCartModel.IsRental
          ? _localizationService.GetResource("ShoppingCart.AddToCart")
          : _localizationService.GetResource("ShoppingCart.Rent")
      };
      return View("Default",  cartAjaxButtonModel);
    }
  }
}
