using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public interface IPackagesFactory
    {
        PackagesListModel PreparePackagesListModel(PackagesSearchModel searchModel);

        PackagesModel PreparePackagesModel(Packages packages, PackagesModel packagesModel);

        PackageProductListModel PreparePackageProductListModel(PackageProductSearchModel searchModel, Packages packages);

        AddProductToCategoryListModel PrepareAddProductToPackageListModel(AddProductToPackageSearchModel searchModel);

        AddProductToPackageSearchModel PrepareAddProductToPackageSearchModel(AddProductToPackageSearchModel searchModel);

        PackageProductModel PreparePackageProductModel(PackageProduct packageProduct);
    }
}
