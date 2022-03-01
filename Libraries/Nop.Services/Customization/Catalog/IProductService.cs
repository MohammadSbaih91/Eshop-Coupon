using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial interface IProductService
    {
        int[] ParseRequiredAnyOneFromOtherProductIds(Product product);

        /// <summary>
        /// Gets all products displayed on the home page
        /// </summary>
        /// <returns>Products</returns>
        IPagedList<Product> GetProductsDisplayedOnHomePage(IList<int> categoryIds,int pageIndex = 0, int pageSize = int.MaxValue);

        bool IsReviewAddeByCustomerToProduct(int customerId, int productId);
    }
}