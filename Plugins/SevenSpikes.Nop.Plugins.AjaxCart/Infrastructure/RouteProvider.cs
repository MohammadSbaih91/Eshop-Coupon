
using Microsoft.AspNetCore.Routing;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Web.Framework.Localization;
using SevenSpikes.Nop.Framework.Routing;
using SevenSpikes.Nop.Framework.ViewLocations;
using SevenSpikes.Nop.Plugins.AjaxCart.Domain;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Infrastructure
{
  public class RouteProvider : BaseRouteProvider
  {
    public RouteProvider()
      : base()
    {
    }

    protected override void SetNopcommerceSettings()
    {
      var shoppingCartSettings = EngineContext.Current.Resolve<ShoppingCartSettings>();
      var isettingService = EngineContext.Current.Resolve<ISettingService>();
      if (!EngineContext.Current.Resolve<NopAjaxCartSettings>().EnableNopAjaxCart || !shoppingCartSettings.DisplayWishlistAfterAddingProduct && !shoppingCartSettings.DisplayCartAfterAddingProduct)
        return;
      shoppingCartSettings.DisplayCartAfterAddingProduct = false;
      shoppingCartSettings.DisplayWishlistAfterAddingProduct = false;
      isettingService.SaveSetting( shoppingCartSettings, 0);
    }

    protected override void RegisterDuplicateControllers(IViewLocationsManager viewLocationsManager)
    {
      var duplicateControllerInfoList = new List<DuplicateControllerInfo>();
      var duplicateControllerInfo1 = new DuplicateControllerInfo()
      {
        DuplicateControllerName = "NopAjaxCartShoppingCart",
        DuplicateOfControllerName = "ShoppingCart"
      };
      var duplicateControllerInfo2 = new DuplicateControllerInfo()
      {
        DuplicateControllerName = "NopAjaxCartCatalog",
        DuplicateOfControllerName = "Product"
      };
      duplicateControllerInfoList.Add(duplicateControllerInfo1);
      duplicateControllerInfoList.Add(duplicateControllerInfo2);
      viewLocationsManager.AddDuplicateControllers(duplicateControllerInfoList);
    }

    protected override void RegisterRoutesAccessibleByName(IRouteBuilder routes)
    {
      base.RegisterRoutesAccessibleByName(routes);
      routes.MapLocalizedRoute("AddProductFromProductDetailsPageToCartAjax", "AddProductFromProductDetailsPageToCartAjax/",  new
      {
        controller = "NopAjaxCartShoppingCart",
        action = "AddProductFromProductDetailsPageToCartAjax"
      });
      routes.MapLocalizedRoute("AddProductToCartAjax", "AddProductToCartAjax/",  new
      {
        controller = "NopAjaxCartShoppingCart",
        action = "AddProductToCartAjax"
      });
      routes.MapLocalizedRoute("MiniShoppingCart", "MiniShoppingCart/",  new
      {
        controller = "NopAjaxCartShoppingCart",
        action = "MiniShoppingCart"
      });
      routes.MapLocalizedRoute("NopAjaxCartFlyoutShoppingCart", "NopAjaxCartFlyoutShoppingCart/",  new
      {
        controller = "NopAjaxCartShoppingCart",
        action = "NopAjaxCartFlyoutShoppingCart"
      });
      routes.MapLocalizedRoute("CheckIfProductOrItsAssociatedProductsHasAttributes", "CheckIfProductOrItsAssociatedProductsHasAttributes/",  new
      {
        controller = "NopAjaxCartCatalog",
        action = "CheckIfProductOrItsAssociatedProductsHasAttributes"
      });
      routes.MapLocalizedRoute("GetMiniProductDetailsView", "GetMiniProductDetailsView/",  new
      {
        controller = "NopAjaxCartCatalog",
        action = "GetMiniProductDetailsView"
      });
    }

    protected override string PluginSystemName => "SevenSpikes.Nop.Plugins.AjaxCart";

    protected override bool ShouldAddPluginViewLocationsBeforeNopViewLocations() => true;
  }
}
