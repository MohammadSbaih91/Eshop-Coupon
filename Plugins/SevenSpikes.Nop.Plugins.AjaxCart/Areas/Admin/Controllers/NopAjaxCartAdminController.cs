
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using SevenSpikes.Nop.Framework.Areas.Admin.ControllerAttributes;
using SevenSpikes.Nop.Framework.Areas.Admin.Controllers;
using SevenSpikes.Nop.Framework.Areas.Admin.Helpers;
using SevenSpikes.Nop.Plugins.AjaxCart.Areas.Admin.Extensions;
using SevenSpikes.Nop.Plugins.AjaxCart.Areas.Admin.Models;
using SevenSpikes.Nop.Plugins.AjaxCart.Domain;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Areas.Admin.Controllers
{
  [ManagePluginsAdminAuthorize("SevenSpikes.Nop.Plugins.AjaxCart", false)]
  public class NopAjaxCartAdminController : Base7SpikesAdminController
  {
    private readonly WidgetSettings _widgetSettings;
    private readonly ISettingService _settingService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;

    public NopAjaxCartAdminController(
      WidgetSettings widgetSettings,
      ISettingService settingService,
      ICustomerActivityService customerActivityService,
      ILocalizationService localizationService)
    {
      _widgetSettings = widgetSettings;
      _settingService = settingService;
      _customerActivityService = customerActivityService;
      _localizationService = localizationService;
    }

    public ActionResult Settings()
    {
      var scopeConfiguration = StoreContext.ActiveStoreScopeConfiguration;
      var ajaxCartSettings = _settingService.LoadSetting<NopAjaxCartSettings>(scopeConfiguration);
      var model = ajaxCartSettings.ToModel();
      model.ActiveStoreScopeConfiguration = scopeConfiguration;
      model.EnableNopAjaxCart = model.EnableNopAjaxCart && _widgetSettings.ActiveWidgetSystemNames.Contains("SevenSpikes.Nop.Plugins.AjaxCart");
      if (scopeConfiguration > 0)
      {
        var scopeSettingsHelper = new StoreScopeSettingsHelper<NopAjaxCartSettings>(ajaxCartSettings, scopeConfiguration, _settingService);
        model.EnableNopAjaxCart_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.EnableNopAjaxCart) ? 1 : 0) != 0;
        model.EnableOnMobileDevices_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.EnableOnMobileDevices) ? 1 : 0) != 0;
        model.EnableOnProductPage_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.EnableOnProductPage) ? 1 : 0) != 0;
        model.EnableOnCatalogPages_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.EnableOnCatalogPages) ? 1 : 0) != 0;
        model.EnableProductQuantityTextBox_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.EnableProductQuantityTextBox) ? 1 : 0) != 0;
        model.WishlistMenuLinkSelector_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.WishlistMenuLinkSelector) ? 1 : 0) != 0;
        model.AddToWishlistButtonSelector_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.AddToWishlistButtonSelector) ? 1 : 0) != 0;
        model.ShoppingCartMenuLinkSelector_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.ShoppingCartMenuLinkSelector) ? 1 : 0) != 0;
        model.FlyoutCartPanelSelector_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.FlyoutCartPanelSelector) ? 1 : 0) != 0;
        model.ProductPageAddToCartButtonSelector_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.ProductPageAddToCartButtonSelector) ? 1 : 0) != 0;
        model.ProductBoxAddToCartButtonSelector_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.ProductBoxAddToCartButtonSelector) ? 1 : 0) != 0;
        model.ProductBoxProductItemElementSelector_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.ProductBoxProductItemElementSelector) ? 1 : 0) != 0;
        model.ProductAddedToCartImageSize_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.ProductAddedToCartImageSize) ? 1 : 0) != 0;
        model.EnableRelatedProductsInPopup_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.EnableRelatedProductsInPopup) ? 1 : 0) != 0;
        model.NumberOfRelatedProductsInPopup_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.NumberOfRelatedProductsInPopup) ? 1 : 0) != 0;
        model.EnableCrossSellProductsInPopup_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.EnableCrossSellProductsInPopup) ? 1 : 0) != 0;
        model.NumberOfCrossSellProductsInPopup_OverrideForStore = (scopeSettingsHelper.SettingExists(x => x.NumberOfCrossSellProductsInPopup) ? 1 : 0) != 0;
      }
      model.IsTrialVersion = false;
      return View(nameof (Settings),  model);
    }

    [HttpPost]
    public ActionResult Settings(NopAjaxCartSettingsModel nopAjaxCartSettingsModel)
    {
      if (!ModelState.IsValid)
        RedirectToAction(nameof (Settings));
      if (nopAjaxCartSettingsModel.EnableNopAjaxCart && !_widgetSettings.ActiveWidgetSystemNames.Contains("SevenSpikes.Nop.Plugins.AjaxCart"))
      {
        _widgetSettings.ActiveWidgetSystemNames.Add("SevenSpikes.Nop.Plugins.AjaxCart");
        _settingService.SaveSetting( _widgetSettings, 0);
      }
      var scopeConfiguration = StoreContext.ActiveStoreScopeConfiguration;
      var scopeSettingsHelper = new StoreScopeSettingsHelper<NopAjaxCartSettings>(nopAjaxCartSettingsModel.ToEntity(), scopeConfiguration, _settingService);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.EnableNopAjaxCart_OverrideForStore ? 1 : 0) != 0, x => x.EnableNopAjaxCart);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.EnableOnMobileDevices_OverrideForStore ? 1 : 0) != 0, x => x.EnableOnMobileDevices);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.EnableOnProductPage_OverrideForStore ? 1 : 0) != 0, x => x.EnableOnProductPage);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.EnableOnCatalogPages_OverrideForStore ? 1 : 0) != 0, x => x.EnableOnCatalogPages);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.EnableProductQuantityTextBox_OverrideForStore ? 1 : 0) != 0, x => x.EnableProductQuantityTextBox);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.WishlistMenuLinkSelector_OverrideForStore ? 1 : 0) != 0, x => x.WishlistMenuLinkSelector);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.AddToWishlistButtonSelector_OverrideForStore ? 1 : 0) != 0, x => x.AddToWishlistButtonSelector);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.ShoppingCartMenuLinkSelector_OverrideForStore ? 1 : 0) != 0, x => x.ShoppingCartMenuLinkSelector);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.FlyoutCartPanelSelector_OverrideForStore ? 1 : 0) != 0, x => x.FlyoutCartPanelSelector);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.ProductPageAddToCartButtonSelector_OverrideForStore ? 1 : 0) != 0, x => x.ProductPageAddToCartButtonSelector);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.ProductBoxAddToCartButtonSelector_OverrideForStore ? 1 : 0) != 0, x => x.ProductBoxAddToCartButtonSelector);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.ProductBoxProductItemElementSelector_OverrideForStore ? 1 : 0) != 0, x => x.ProductBoxProductItemElementSelector);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.ProductAddedToCartImageSize_OverrideForStore ? 1 : 0) != 0, x => x.ProductAddedToCartImageSize);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.EnableRelatedProductsInPopup_OverrideForStore ? 1 : 0) != 0, x => x.EnableRelatedProductsInPopup);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.NumberOfRelatedProductsInPopup_OverrideForStore ? 1 : 0) != 0, x => x.NumberOfRelatedProductsInPopup);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.EnableCrossSellProductsInPopup_OverrideForStore ? 1 : 0) != 0, x => x.EnableCrossSellProductsInPopup);
      scopeSettingsHelper.SaveStoreSetting((nopAjaxCartSettingsModel.NumberOfCrossSellProductsInPopup_OverrideForStore ? 1 : 0) != 0, x => x.NumberOfCrossSellProductsInPopup);
      _settingService.ClearCache();
      _customerActivityService.InsertActivity("EditNopAjaxCartSettings", "Edit Nop Ajax Cart Settings", null);
      SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"), true);
      SaveSelectedTabName("", true);
      return RedirectToAction(nameof (Settings));
    }
  }
}
