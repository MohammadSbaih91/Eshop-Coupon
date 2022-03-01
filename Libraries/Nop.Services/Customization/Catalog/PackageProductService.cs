using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Events;
using System.Linq;
using System.Collections.Generic;
using System;
using Nop.Core;

namespace Nop.Services.Catalog
{
    public class PackageProductService : IPackageProductService
    {
        #region Fields
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<PackageProduct> _packageProductrepository;
        #endregion

        #region Ctor
        public PackageProductService(IEventPublisher eventPublisher,
            IRepository<PackageProduct> packageProductrepository)
        {
            _eventPublisher = eventPublisher;
            _packageProductrepository = packageProductrepository;
        }
        #endregion

        #region Methods
        public virtual IPagedList<PackageProduct> GetPackageProduct(int packageId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _packageProductrepository.Table;

            query = from sa in query
                    where sa.PackageId == packageId
                    orderby sa.DisplayOrder, sa.Id
                    select sa;
            return new PagedList<PackageProduct>(query, pageIndex, pageSize);
        }

        public virtual PackageProduct GetPackageProductById(int Id)
        {
            if (Id == 0)
                return null;

            return _packageProductrepository.GetById(Id);
        }

        public virtual IList<PackageProduct> GetPackageProductList()
        {
            var query = _packageProductrepository.Table;

            query = from sa in query
                    orderby sa.DisplayOrder, sa.Id
                    select sa;

            return new List<PackageProduct>(query);
        }

        public virtual IList<PackageProduct> GetPackageProductByPackageId(int packageId)
        {
            var query = _packageProductrepository.Table;

            query = from sa in query
                    where sa.PackageId == packageId
                    orderby sa.DisplayOrder, sa.Id
                    select sa;

            return new List<PackageProduct>(query);
        }

        public virtual PackageProduct FindPackageProduct(IList<PackageProduct> source, int productId, int packageId)
        {
            foreach (var item in source)
                if (item.ProductId == productId && item.PackageId == packageId)
                    return item;

            return null;
        }

        public virtual void InsertPackageProduct(PackageProduct packageProduct)
        {
            if (packageProduct == null)
                throw new ArgumentNullException(nameof(packageProduct));

            _packageProductrepository.Insert(packageProduct);

            //event notification
            _eventPublisher.EntityInserted(packageProduct);
        }

        public virtual void UpdatePackageProduct(PackageProduct packageProduct)
        {
            if (packageProduct == null)
                throw new ArgumentNullException(nameof(packageProduct));

            _packageProductrepository.Update(packageProduct);

            _eventPublisher.EntityInserted(packageProduct);
        }

        public virtual void DeletePackageProduct(PackageProduct packageProduct)
        {
            if (packageProduct == null)
                throw new ArgumentNullException(nameof(packageProduct));

            _packageProductrepository.Delete(packageProduct);

            _eventPublisher.EntityInserted(packageProduct);
        }
        #endregion
    }
}
