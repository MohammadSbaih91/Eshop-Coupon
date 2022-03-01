
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Plugins.AjaxCart.Domain;
using System.Collections.Generic;
using System.Linq;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Components
{
  [ViewComponent(Name = "NopAjaxCartPopupRelatedProducts")]
  public class NopAjaxCartPopupRelatedProductsComponent : ViewComponent
  {
    private readonly IProductModelFactory _productModelFactory;
    private readonly IProductService _productService;
    private readonly IAclService _aclService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly NopAjaxCartSettings _nopAjaxCartSettings;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;

    public NopAjaxCartPopupRelatedProductsComponent(
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
      foreach (var productsById in _productService.GetProductsByIds(_productService.GetRelatedProductsByProductId1(productId, false).Take(_nopAjaxCartSettings.NumberOfRelatedProductsInPopup).ToList().Select(x => x.ProductId2).ToArray()))
      {
        if (_aclService.Authorize( productsById) && _storeMappingService.Authorize( productsById))
          productList.Add(productsById);
      }
      return View<IList<ProductOverviewModel>>("PopupRelatedProducts",  _productModelFactory.PrepareProductOverviewModels(productList, true, true, new int?(_nopAjaxCartSettings.ProductAddedToCartImageSize), false, false).ToList());
    }
  }
}
