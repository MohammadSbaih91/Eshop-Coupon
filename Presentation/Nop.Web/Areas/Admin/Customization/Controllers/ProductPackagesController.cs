using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Customization.Controllers
{
    public class ProductPackagesController : BaseAdminController
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly IPackagesService _packagesService;
        private readonly IPermissionService _permissionService;
        private readonly IPackagesFactory _packagesFactory;
        private readonly ICategoryModelFactory _categoryModelFactory;
        private readonly IPackageProductService _packageProductService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        #endregion

        #region Ctor
        public ProductPackagesController(ILocalizationService localizationService,
            IPackagesService packagesService,
            IPermissionService permissionService,
            IPackagesFactory packagesFactory,
            ICategoryModelFactory categoryModelFactory,
            IPackageProductService packageProductService,
            IProductService productService,
            ICategoryService categoryService)
        {
            _localizationService = localizationService;
            _packagesService = packagesService;
            _permissionService = permissionService;
            _packagesFactory = packagesFactory;
            _categoryModelFactory = categoryModelFactory;
            _packageProductService = packageProductService;
            _productService = productService;
            _categoryService = categoryService;
        }
        #endregion

        #region Methods
        #region Package
        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            return View(new PackagesSearchModel());
        }

        [HttpPost]
        public virtual IActionResult List(PackagesSearchModel searchModel)
        {
            //prepare model
            var model = _packagesFactory.PreparePackagesListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _packagesFactory.PreparePackagesModel(null, new PackagesModel());
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(PackagesModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                string categoryIds = string.Join(",", model.SelectedCategoryIds);
                var package = new Packages()
                {
                    Name = model.Name,
                    DisplayOrder = model.DisplayOrder,
                    CategoryIds = categoryIds,
                    Published = model.Published
                };
                _packagesService.InsertPackages(package);

                SuccessNotification(_localizationService.GetResource("Admin.Packages.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = package.Id });
            }

            //if we got this far, something failed, redisplay form
            model = _packagesFactory.PreparePackagesModel(null, model);
            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a package with the specified id
            var package = _packagesService.GetPackagesById(id);
            if (package == null)
                return RedirectToAction("List");

            //prepare model
            var model = _packagesFactory.PreparePackagesModel(package, null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(PackagesModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var package = _packagesService.GetPackagesById(model.Id);
            if (package == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                string categoryIds = string.Join(",", model.SelectedCategoryIds);
                package.Name = model.Name;
                package.DisplayOrder = model.DisplayOrder;
                package.CategoryIds = categoryIds;
                package.Published = model.Published;

                _packagesService.UpdatePackages(package);

                SuccessNotification(_localizationService.GetResource("Admin.Packages.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = package.Id });
            }
            model = _packagesFactory.PreparePackagesModel(null, model);
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a amalgamationGroup with the specified id
            var package = _packagesService.GetPackagesById(id);
            if (package == null)
                return RedirectToAction("List");

            _packagesService.DeletePackages(package);

            SuccessNotification(_localizationService.GetResource("Admin.Packages.Deleted"));

            return new NullJsonResult();
        }
        #endregion

        #region Package Product
        [HttpPost]
        public virtual IActionResult PackageProductList(PackageProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedKendoGridJson();

            var package = _packagesService.GetPackagesById(searchModel.PackageId)
                ?? throw new ArgumentException("No package found with the specified id");

            //prepare model
            var model = _packagesFactory.PreparePackageProductListModel(searchModel, package);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult PackageProductAddPopupList(AddProductToPackageSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _packagesFactory.PrepareAddProductToPackageListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult PackageProductAddPopup(int packageId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //prepare model
            var model = _packagesFactory.PrepareAddProductToPackageSearchModel(new AddProductToPackageSearchModel());

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual IActionResult PackageProductAddPopup(AddProductToPackageModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //get selected products
            var selectedProducts = _productService.GetProductsByIds(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                var existingPackageProducts = _packageProductService.GetPackageProduct(model.PackageId);
                foreach (var product in selectedProducts)
                {
                    //whether product package with such parameters already exists
                    if (_packageProductService.FindPackageProduct(existingPackageProducts, product.Id, model.PackageId) != null)
                        continue;

                    //insert the new product category mapping
                    var packageProduct = new PackageProduct()
                    {
                        PackageId = model.PackageId,
                        ProductId = product.Id,
                        DisplayOrder = 1
                    };
                    _packageProductService.InsertPackageProduct(packageProduct);
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddProductToPackageSearchModel());
        }

        public virtual IActionResult PackageProductEditPopup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var PackageProduct = _packageProductService.GetPackageProductById(id)
                ?? throw new ArgumentException("No package found with the specified id");
            var model = _packagesFactory.PreparePackageProductModel(PackageProduct);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult PackageProductEditPopup(PackageProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var packageProduct = _packageProductService.GetPackageProductById(model.Id)
                ?? throw new ArgumentException("No package product found with the specified id");
            string discountIds = string.Join(",", model.SelectedDiscountIds);
            if (!string.IsNullOrEmpty(discountIds))
            {
                packageProduct.DiscountIds = discountIds;
            }
            packageProduct.DisplayOrder = model.DisplayOrder;
            _packageProductService.UpdatePackageProduct(packageProduct);
            ViewBag.RefreshPage = true;
            return View(new PackageProductModel());
        }

        public virtual IActionResult PackageProductDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var packageProduct = _packageProductService.GetPackageProductById(id)
                ?? throw new ArgumentException("No package product found with the specified id", nameof(id));

            _packageProductService.DeletePackageProduct(packageProduct);

            return new NullJsonResult();
        }
        
        #endregion
        #endregion
    }
}
