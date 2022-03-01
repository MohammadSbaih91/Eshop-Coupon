
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Controllers;
using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Plugins.AjaxCart.Domain;
using SevenSpikes.Nop.Plugins.AjaxCart.Models;
using System.Collections.Generic;
using System.Linq;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Controllers
{
  public class NopAjaxCartController : BasePublicController
  {
    private readonly NopAjaxCartSettings _nopAjaxCartSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IProductService _productService;
    private readonly IStoreContext _storeContext;
    private readonly IPermissionService _permissionService;
    private readonly IEncryptionService _encryptionService;
    private readonly IStaticCacheManager _staticCacheManager;

    public NopAjaxCartController(
      NopAjaxCartSettings nopAjaxCartSettings,
      ILocalizationService localizationService,
      IProductService productService,
      IStoreContext storeContext,
      IPermissionService permissionService,
      IEncryptionService encryptionService,
      IStaticCacheManager staticCacheManager)
    {
      _nopAjaxCartSettings = nopAjaxCartSettings;
      _localizationService = localizationService;
      _productService = productService;
      _storeContext = storeContext;
      _permissionService = permissionService;
      _encryptionService = encryptionService;
      _staticCacheManager = staticCacheManager;
    }

    [HttpGet]
    public ActionResult GetAjaxCartButtonsAjax() => new EmptyResult();

    [HttpPost]
    public ActionResult GetAjaxCartButtonsAjax(
      [FromBody] List<AddToCartButtonInfoModel> addToCartButtonsInfos)
    {
      if (addToCartButtonsInfos == null || addToCartButtonsInfos.Count < 1)
        return new EmptyResult();
      var cartButtonsModel = new AjaxCartButtonsModel();
      var str =
        $"nop.pres.7spikes.ajaxcart{_encryptionService.CreatePasswordHash(string.Join(",", addToCartButtonsInfos.Select(cartButtonInfo => $"{cartButtonInfo.ProductId},{cartButtonInfo.IsProductPage},{cartButtonInfo.ButtonValue}")), "SS_AjaxCart_EK_1", "SHA1")}-{_storeContext.CurrentStore.Id}-{_nopAjaxCartSettings.EnableOnCatalogPages}-{_nopAjaxCartSettings.EnableOnProductPage}";
      cartButtonsModel.AddProductToCartAjaxButtonModels = _staticCacheManager.Get(str,  () =>
      {
        var cartAjaxButtonModelList = new List<AddProductToCartAjaxButtonModel>();
        IList<Product> productList =  new List<Product>();
        if (addToCartButtonsInfos.Any( x => !x.IsProductPage) && _nopAjaxCartSettings.EnableOnCatalogPages)
          productList = _productService.GetProductsByIds(addToCartButtonsInfos.Select(x => x.ProductId).ToArray());
        foreach (var toCartButtonsInfo in addToCartButtonsInfos)
        {
          var button = toCartButtonsInfo;
          if (button.IsProductPage && _nopAjaxCartSettings.EnableOnProductPage)
          {
            var cartAjaxButtonModel = CreateAddProductToCartAjaxButtonModel(button);
            cartAjaxButtonModelList.Add(cartAjaxButtonModel);
          }
          else if (!button.IsProductPage && _nopAjaxCartSettings.EnableOnCatalogPages)
          {
            var product1 = productList.FirstOrDefault(p => p.Id == button.ProductId);
            if (product1 == null) continue;
            var cartAjaxButtonModel = CreateAddProductToCartAjaxButtonModel(button);
            var num = 1;
            switch (product1.ProductType)
            {
              case ProductType.GroupedProduct:
              {
                var product2 = _productService.GetAssociatedProducts(product1.Id, _storeContext.CurrentStore.Id, 0, false).FirstOrDefault();
                if (product2 != null)
                {
                  num = product2.OrderMinimumQuantity;
                  product1 = product2;
                }

                break;
              }
              case  ProductType.SimpleProduct:
                num = product1.OrderMinimumQuantity;
                break;
            }
            foreach (var allowedQuantity in _productService.ParseAllowedQuantities(product1))
              cartAjaxButtonModel.AllowedQuantities.Add(new SelectListItem()
              {
                Text = allowedQuantity.ToString(),
                Value = allowedQuantity.ToString()
              });
            cartAjaxButtonModel.DefaultProductMinimumQuantity = num;
            cartAjaxButtonModelList.Add(cartAjaxButtonModel);
          }
        }
        return cartAjaxButtonModelList;
      }, new int?());
      return PartialView("AddProductToCartAjaxButton",  cartButtonsModel);
    }

    private AddProductToCartAjaxButtonModel CreateAddProductToCartAjaxButtonModel(
      AddToCartButtonInfoModel button)
    {
      var cartAjaxButtonModel = new AddProductToCartAjaxButtonModel()
      {
        IsProductPage = button.IsProductPage,
        ButtonValue = button.ButtonValue,
        ProductId = button.ProductId
      };
      if (string.IsNullOrEmpty(cartAjaxButtonModel.ButtonValue))
        cartAjaxButtonModel.ButtonValue = _localizationService.GetResource("ShoppingCart.AddToCart");
      return cartAjaxButtonModel;
    }

    public ActionResult GetMiniProductDetailsViewAddProductToCartAjaxButton(
      ProductDetailsModel.AddToCartModel addToCartModel)
    {
      var cartAjaxButtonModel = new AddProductToCartAjaxButtonModel
      {
        ProductId = addToCartModel.ProductId,
        ButtonValue = !addToCartModel.IsRental
          ? (!addToCartModel.AvailableForPreOrder
            ? _localizationService.GetResource("ShoppingCart.AddToCart")
            : _localizationService.GetResource("ShoppingCart.PreOrder"))
          : _localizationService.GetResource("ShoppingCart.Rent")
      };
      return PartialView("Default",  cartAjaxButtonModel);
    }
  }
}
