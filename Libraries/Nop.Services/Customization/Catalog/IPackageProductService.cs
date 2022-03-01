using Nop.Core;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;

namespace Nop.Services.Catalog
{
    public interface IPackageProductService
    {
        IPagedList<PackageProduct> GetPackageProduct(int packageId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        PackageProduct GetPackageProductById(int Id);

        IList<PackageProduct> GetPackageProductList();

        IList<PackageProduct> GetPackageProductByPackageId(int packageId);

        PackageProduct FindPackageProduct(IList<PackageProduct> source, int productId, int packageId);

        void InsertPackageProduct(PackageProduct packageProduct);

        void UpdatePackageProduct(PackageProduct packageProduct);

        void DeletePackageProduct(PackageProduct packageProduct);
    }
}
