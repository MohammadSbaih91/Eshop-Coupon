using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Customization.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Components
{
    public class PackagesListViewComponent : NopViewComponent
    {
        #region Fields
        private readonly IPackagesService _packagesService;
        private readonly IPackageProductService _packageProductService;
        private readonly IProductService _productService;
        #endregion

        #region Ctor
        public PackagesListViewComponent(IPackagesService packagesService,
            IPackageProductService packageProductService,
            IProductService productService)
        {
            _packagesService = packagesService;
            _packageProductService = packageProductService;
            _productService = productService;
        }
        #endregion

        #region Methods
        public IViewComponentResult Invoke(int categoryId)
        {
            var packages = _packagesService.GetPackagesByCategoryId(categoryId);
            var model = new List<PackageModel>();
            if (packages != null)
            {
                // package list
                foreach (var item in packages)
                {
                    var packageModel = new PackageModel();
                    packageModel.Id = item.Id;
                    packageModel.PackageName = item.Name;
                    var packageProducts = _packageProductService.GetPackageProductByPackageId(item.Id);
                    if (packageProducts != null && packageProducts.Count() > 0)
                    {
                        //Product list
                        foreach (var packageProduct in packageProducts)
                        {
                            var productModel = new ProductOverviewModel()
                            {
                                Id = packageProduct.ProductId,
                                Name = _productService.GetProductById(packageProduct.ProductId).Name,
                            };
                            packageModel.Products.Add(productModel);
                        }
                        model.Add(packageModel);
                    }
                }
            }
            return View(model);
        }
        #endregion
    }
}
