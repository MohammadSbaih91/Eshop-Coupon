
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Plugins.AjaxCart.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Components
{
  [ViewComponent(Name = "NopAjaxCartPopupCrossSellProducts")]
  public class NopAjaxCartPopupCrossSellProductsComponent : ViewComponent
  {
    private readonly IProductModelFactory _productModelFactory;
    private readonly IProductService _productService;
    private readonly IAclService _aclService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly NopAjaxCartSettings _nopAjaxCartSettings;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;

    public NopAjaxCartPopupCrossSellProductsComponent(
      IProductModelFactory productModelFactory,
      IProductService productService,
      IAclService aclService,
      IStoreMappingService storeMappingService,
      NopAjaxCartSettings nopAjaxCartSettings,
      IStoreContext storeContext,
      IWorkContext workContext)
    {
      _productModelFactory = productModelFactory;
      _productService = productService;
      _nopAjaxCartSettings = nopAjaxCartSettings;
      _storeContext = storeContext;
      _workContext = workContext;
      _aclService = aclService;
      _storeMappingService = storeMappingService;
    }

    public IViewComponentResult Invoke(int productId)
    {
      var productList = new List<Product>();
      var productsByProductId1 = _productService.GetCrossSellProductsByProductId1(productId, false);
      var shoppingCartProductIds = GetShoppingCartProductIds();
      var predicate = (Func<CrossSellProduct, bool>) (x => !( shoppingCartProductIds).Contains(x.ProductId2));
      foreach (var productsById in _productService.GetProductsByIds(productsByProductId1.Where(predicate).Select( x => x.ProductId2).ToArray()))
      {
        if (_aclService.Authorize(productsById) && _storeMappingService.Authorize(productsById))
        {
          productList.Add(productsById);
          if (productList.Count == _nopAjaxCartSettings.NumberOfCrossSellProductsInPopup)
            break;
        }
      }
      return View<IList<ProductOverviewModel>>("PopupCrossSellProducts", _productModelFactory
        .PrepareProductOverviewModels( productList, true, true, new int?(_nopAjaxCartSettings.ProductAddedToCartImageSize), false, false)
        .ToList());
    }

    private int[] GetShoppingCartProductIds() => 
      _workContext.CurrentCustomer.ShoppingCartItems
        .Where(sci =>(int) sci.ShoppingCartType == 1).LimitPerStore(_storeContext.CurrentStore.Id)
        .ToList().Select( x => x.ProductId).ToArray();
  }
}
