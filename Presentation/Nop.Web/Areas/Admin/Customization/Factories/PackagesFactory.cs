using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Web.Areas.Admin.Factories
{
    public class PackagesFactory : IPackagesFactory
    {
        #region Fields
        private readonly IPackagesService _packagesService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IPackageProductService _packageProductService;
        private readonly IProductService _productService;
        private readonly IDiscountService _discountService;
        private readonly IDiscountSupportedModelFactory _discountSupportedModelFactory;
        #endregion

        #region Ctor
        public PackagesFactory(IPackagesService packagesService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IPackageProductService packageProductService,
            IProductService productService,
            IDiscountService discountService,
            IDiscountSupportedModelFactory discountSupportedModelFactory)
        {
            _packagesService = packagesService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _packageProductService = packageProductService;
            _productService = productService;
            _discountService = discountService;
            _discountSupportedModelFactory = discountSupportedModelFactory;
        }
        #endregion

        #region Methods
        public PackagesListModel PreparePackagesListModel(PackagesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var package = _packagesService.GetPackages(
                    name: searchModel.Name,
                    pageIndex: searchModel.Page - 1,
                    pageSize: searchModel.PageSize);

            //prepare list model
            var model = new PackagesListModel
            {
                Data = package.Select(p =>
                {
                    return new PackagesModel()
                    {
                        Id = p.Id,
                        Name = p.Name,
                        DisplayOrder = p.DisplayOrder,
                        CategoryIds = p.CategoryIds,
                        Published = p.Published
                    };
                }),
                Total = package.TotalCount
            };

            return model;
        }

        public PackagesModel PreparePackagesModel(Packages packages, PackagesModel packagesModel)
        {
            if (packages == null)
            {
                var model = new PackagesModel()
                {
                    Name = packagesModel.Name,
                    DisplayOrder = packagesModel.DisplayOrder,
                    CategoryIds = packagesModel.CategoryIds,
                    Published = packagesModel.Published
                };
                //prepare model categories
                _baseAdminModelFactory.PrepareCategories(model.AvailableCategories, false);
                foreach (var categoryItem in model.AvailableCategories)
                {
                    categoryItem.Selected = int.TryParse(categoryItem.Value, out var categoryId)
                        && model.SelectedCategoryIds.Contains(categoryId);
                }
                return model;
            }
            else
            {
                var categoryIds = new List<int>();
                categoryIds = packages.CategoryIds.Split(',').Select(int.Parse).ToList();
                var model = new PackagesModel()
                {
                    Id = packages.Id,
                    Name = packages.Name,
                    DisplayOrder = packages.DisplayOrder,
                    CategoryIds = packages.CategoryIds,
                    Published = packages.Published,
                    SelectedCategoryIds = categoryIds,
                };
                //prepare model categories
                _baseAdminModelFactory.PrepareCategories(model.AvailableCategories, false);
                foreach (var categoryItem in model.AvailableCategories)
                {
                    categoryItem.Selected = int.TryParse(categoryItem.Value, out var categoryId)
                        && model.SelectedCategoryIds.Contains(categoryId);
                }
                return model;
            }
        }

        public virtual PackageProductListModel PreparePackageProductListModel(PackageProductSearchModel searchModel, Packages packages)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (packages == null)
                throw new ArgumentNullException(nameof(packages));

            var packageProducts = _packageProductService.GetPackageProduct(
                    packageId: searchModel.PackageId,
                    pageIndex: searchModel.Page - 1,
                    pageSize: searchModel.PageSize);


            var listmodel = new List<PackageProductModel>();
            foreach (var item in packageProducts)
            {
                var discountids = new List<int>();
                if (!string.IsNullOrEmpty(item.DiscountIds))
                {
                    discountids = item.DiscountIds.Split(",").Select(int.Parse).ToList();
                }
                var disname = "";
                foreach (var discountId in discountids)
                {
                    var dicount = _discountService.GetDiscountById(discountId);
                    disname = disname + ", " + dicount.Name;
                }
                var data = new PackageProductModel()
                {
                    Id = item.Id,
                    PackageId = item.PackageId,
                    ProductId = item.ProductId,
                    ProductName = _productService.GetProductById(item.ProductId)?.Name,
                    DiscountIds = item.DiscountIds,
                    DiscountName = disname,
                    DisplayOrder = item.DisplayOrder
                };
                listmodel.Add(data);
            }

            var model = new PackageProductListModel()
            {
                Data = listmodel,
                Total = packageProducts.TotalCount
            };
            return model;
        }

        public virtual AddProductToCategoryListModel PrepareAddProductToPackageListModel(AddProductToPackageSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
            var products = _productService.SearchProducts(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerId: searchModel.SearchManufacturerId,
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new AddProductToCategoryListModel
            {
                //fill in model values from the entity
                Data = products.Select(product => product.ToModel<ProductModel>()),
                Total = products.TotalCount
            };

            return model;
        }

        public virtual AddProductToPackageSearchModel PrepareAddProductToPackageSearchModel(AddProductToPackageSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available categories
            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            //prepare available manufacturers
            _baseAdminModelFactory.PrepareManufacturers(searchModel.AvailableManufacturers);

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            //prepare available product types
            _baseAdminModelFactory.PrepareProductTypes(searchModel.AvailableProductTypes);

            //prepare page parameters
            searchModel.SetPopupGridPageSize();

            return searchModel;
        }

        public virtual PackageProductModel PreparePackageProductModel(PackageProduct packageProduct)
        {
            if (packageProduct == null)
                throw new ArgumentNullException(nameof(packageProduct));

            var discountIds = new List<int>();
            if (!string.IsNullOrEmpty(packageProduct.DiscountIds))
            {
                discountIds = packageProduct.DiscountIds.Split(',').Select(int.Parse).ToList();
            }
            var model = new PackageProductModel()
            {
                DiscountIds = packageProduct.DiscountIds,
                DisplayOrder = packageProduct.DisplayOrder,
                SelectedDiscountIds = discountIds
            };
            //prepare model discounts
            var availableDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, showHidden: true);
            _discountSupportedModelFactory.PrepareModelDiscounts(model, availableDiscounts);

            return model;
        }
        #endregion
    }
}
