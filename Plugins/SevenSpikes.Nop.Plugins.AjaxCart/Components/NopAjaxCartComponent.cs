using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Security;
using SevenSpikes.Nop.Core.Helpers;
using SevenSpikes.Nop.Plugins.AjaxCart.Domain;
using System;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Components
{
  [ViewComponent(Name = "NopAjaxCart")]
  public class NopAjaxCartComponent : ViewComponent
  {
    private readonly NopAjaxCartSettings _nopAjaxCartSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IProductService _productService;
    private readonly IStoreContext _storeContext;
    private readonly IPermissionService _permissionService;
    private readonly IEncryptionService _encryptionService;
    private readonly IWebHelper _webHelper;
    private readonly IUserAgentHelper _userAgentHelper;
    private IStaticCacheManager _staticCacheManager;

    public NopAjaxCartComponent(
      NopAjaxCartSettings nopAjaxCartSettings,
      ILocalizationService localizationService,
      IProductService productService,
      IStoreContext storeContext,
      IPermissionService permissionService,
      IEncryptionService encryptionService,
      IWebHelper webHelper,
      IUserAgentHelper userAgentHelper)
    {
      _nopAjaxCartSettings = nopAjaxCartSettings;
      _localizationService = localizationService;
      _productService = productService;
      _storeContext = storeContext;
      _permissionService = permissionService;
      _encryptionService = encryptionService;
      _webHelper = webHelper;
      _userAgentHelper = userAgentHelper;
    }

    private IStaticCacheManager StaticCacheManager
    {
      get
      {
        if (_staticCacheManager == null)
          _staticCacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
        return _staticCacheManager;
      }
    }

    public IViewComponentResult Invoke()
    {
      if (!PluginHelper.IsPluginInstalled("SevenSpikes.Nop.Plugins.AjaxCart"))
        return Content("Nop Ajax Cart is not installed.");
      if (!_nopAjaxCartSettings.EnableNopAjaxCart || !_nopAjaxCartSettings.EnableOnProductPage && !_nopAjaxCartSettings.EnableOnCatalogPages || (Request.Query.ContainsKey("mobile") || !_nopAjaxCartSettings.EnableOnMobileDevices && _userAgentHelper.IsMobileDevice()) || (!_permissionService.Authorize(StandardPermissionProvider.DisplayPrices) || !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart)))
        return Content(string.Empty);
      if (!string.IsNullOrWhiteSpace(Request.Cookies["cookie_theme_roller_iframe"]) && _webHelper.GetUrlReferrer() != null && Request.GetDisplayUrl() != null)
      {
        var host = Request.Host;
        if (host.Host == new Uri(_webHelper.GetUrlReferrer()).Host)
          return Content(string.Empty);
      }
      return View("NopAjaxCart");
    }
  }
}
