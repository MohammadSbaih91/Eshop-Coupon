using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Catalog
{
    public partial interface ICategoryService
    {
        IList<Category> GetAllCategoriesShowWithSubCategories(bool showHidden = false);
    }

    public partial class CategoryService : ICategoryService
    {
        public virtual IList<Category> GetAllCategoriesShowWithSubCategories(bool showHidden = false)
        {
            var query = (from item in _categoryRepository.Table
                         where item.Published == true 
                         && item.Deleted == false 
                         && item.ShowWithSubCategories == true
                         orderby item.DisplayOrder, item.Id
                         select item).ToList();

            var categories = query;
            if (!showHidden)
            {
                categories = categories
                    .Where(c => _aclService.Authorize(c) && _storeMappingService.Authorize(c))
                    .ToList();
            }

            return categories;
        }
    }
}
