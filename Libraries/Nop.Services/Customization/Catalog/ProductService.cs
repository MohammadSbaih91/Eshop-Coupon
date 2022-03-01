using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;

namespace Nop.Services.Catalog
{
    public partial interface IProductService
    {
        decimal GetMaxPriceByCategoryId(int categoryId, int manufacturerId = 0);

        IList<Product> GetServiceProducts();

        IPagedList<Product> GetProductsByCategoryFilter(IList<int> categoryIds, bool isShowOnHomeOnly = false, int productTagId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
    }

    /// <summary>
    /// Product service
    /// </summary>
    public partial class ProductService
    {
        public virtual int[] ParseRequiredAnyOneFromOtherProductIds(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrEmpty(product.RequiredAnyOneFromOtherProductIds))
                return new int[0];

            var ids = new List<int>();

            foreach (var idStr in product.RequiredAnyOneFromOtherProductIds
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()))
            {
                if (int.TryParse(idStr, out var id))
                    ids.Add(id);
            }

            return ids.ToArray();
        }

        /// <summary>
        /// Gets all products displayed on the home page
        /// </summary>
        /// <returns>Products</returns>
        public virtual IPagedList<Product> GetProductsDisplayedOnHomePage(IList<int> categoryIds, int pageIndex = 0,int pageSize = int.MaxValue)
        {
            var query = from p in _productRepository.Table
                        orderby p.DisplayOrder, p.Id
                        where p.Published &&
                        !p.Deleted &&
                        p.ShowOnHomePage
                        select p;

            query = query.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p));
            
            var products = new PagedList<Product>(query, pageIndex, pageSize);
            return products;
        }

        public virtual decimal GetMaxPriceByCategoryId(int categoryId,int manufacturerId=0)
        {
            var _productCategory = EngineContext.Current.Resolve<IRepository<ProductCategory>>();
            var query = _productRepository.Table;
            query = query.Where(p => !p.Deleted && p.Published && p.VisibleIndividually);

            query = from p in query
                    from pc in p.ProductCategories.Where(pc => pc.CategoryId == categoryId)
                    select p;

            if (manufacturerId > 0)
            {
                query = from p in query
                        from pc in p.ProductManufacturers.Where(pm => pm.ManufacturerId == manufacturerId)
                        select p;
            }

            if (query.Any())
                return query.Max(p => p.Price);

            return 0;
        }

        public virtual IList<Product> GetServiceProducts()
        {
            var query = from p in _productRepository.Table
                        orderby p.DisplayOrder, p.Id
                        where p.Published &&
                        !p.Deleted &&
                        p.IsService
                        select p;

            query = query.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p));

            var products = new List<Product>(query);
            return products;
        }

        public virtual bool IsReviewAddeByCustomerToProduct(int customerId, int productId)
        {
            var isAdded = false;
            var query = _productReviewRepository.Table.Where(p=>p.CustomerId == customerId && p.ProductId == productId);

            if (query.Any())
                isAdded = true;

            return isAdded;
        }

        public IPagedList<Product> GetProductsByCategoryFilter(IList<int> categoryIds, bool isShowOnHomeOnly = false, int productTagId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from p in _productRepository.Table
                        orderby p.DisplayOrder, p.Id
                        where p.Published &&
                        !p.Deleted
                        select p;

            if (categoryIds.Any())
            {
                query = from p in query
                        from pc in p.ProductCategories.Where(pc => categoryIds.Contains(pc.CategoryId))
                        select p;
            }

            if (isShowOnHomeOnly)
                query = query.Where(p => p.ShowOnHomePage);
            else
            {
                if (productTagId != 0)
                {
                    query = from p in query
                            from pc in p.ProductProductTagMappings.Where(pt => pt.ProductTagId == productTagId)
                            select p;
                }
            }
            var products = new PagedList<Product>(query, pageIndex, pageSize);
            return products;
        }
    }
}