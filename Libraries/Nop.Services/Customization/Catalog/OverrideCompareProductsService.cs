using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Catalog;
using Nop.Core.Http;

namespace Nop.Services.Catalog
{
    public class OverrideCompareProductsService : CompareProductsService
    {
        #region Fields
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        public OverrideCompareProductsService(CatalogSettings catalogSettings,
            IHttpContextAccessor httpContextAccessor,
            IProductService productService) :base(catalogSettings,
            httpContextAccessor,
            productService)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        #region Methods
        /// <summary>
        /// Adds a product to a "compare products" list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        public override void AddProductToCompareList(int productId)
        {
            base.AddProductToCompareList(productId);

            var compareProduct = GetComparedProducts();
            if (compareProduct.Count > 4)
            {
                int count = 0;
                foreach (var product in compareProduct)
                {
                    if (count > 4)
                    {
                        base.RemoveProductFromCompareList(product.Id);
                    }
                    count++;
                }
            }
        }
        #endregion
    }
}
