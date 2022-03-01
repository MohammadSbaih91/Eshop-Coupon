using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Catalog
{
    public partial interface IProductTagService
    {
        IList<ProductTag> GetProductTagByCategoryId(IList<int> categoryIds);
    }


    public partial class ProductTagService : IProductTagService
    {
        public int GetProductCountbyCategoryId(int productTagId,string categoryId)
        {
            var dictionary = _dbContext.QueryFromSql<ProductTagWithCount>($"Exec SP_ProductTagCountByCategoryId '{categoryId}'")
                    .ToDictionary(item => item.ProductTagId, item => item.ProductCount);

            if (dictionary.ContainsKey(productTagId))
                return dictionary[productTagId];

            return 0;
        }

        public IList<ProductTag> GetProductTagByCategoryId(IList<int> categoryIds)
        {

            var categoryId = string.Join(",", categoryIds);

            //get all tags
            var allTags = GetAllProductTags()
                //filter by current store
                .Where(x => GetProductCountbyCategoryId(x.Id, categoryId) > 0)
                //order by product count
                .OrderByDescending(x => GetProductCountbyCategoryId(x.Id, categoryId))
                .ToList();

            return allTags;
        }
    }
}
