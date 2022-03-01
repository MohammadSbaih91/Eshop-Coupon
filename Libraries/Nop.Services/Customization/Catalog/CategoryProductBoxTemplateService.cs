using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Events;

namespace Nop.Services.Customization.Catalog
{
    public interface ICategoryProductBoxTemplateService
    {
        IList<CategoryProductBoxTemplate> GetAllCategoryTemplates();

        CategoryProductBoxTemplate GetCategoryTemplateById(int categoryTemplateId);
    }


    public class CategoryProductBoxTemplateService : ICategoryProductBoxTemplateService
    {
        private readonly IRepository<CategoryProductBoxTemplate> _categoryProductBoxTemplateRepository;

        public CategoryProductBoxTemplateService(IRepository<CategoryProductBoxTemplate> categoryProductBoxTemplateRepository)
        {
            _categoryProductBoxTemplateRepository = categoryProductBoxTemplateRepository;
        }

        /// <summary>
        /// Gets all category templates
        /// </summary>
        /// <returns>Category templates</returns>
        public virtual IList<CategoryProductBoxTemplate> GetAllCategoryTemplates()
        {
            var query = from pt in _categoryProductBoxTemplateRepository.Table
                        orderby pt.DisplayOrder, pt.Id
                        select pt;

            var templates = query.ToList();
            return templates;
        }

        /// <summary>
        /// Gets a category template
        /// </summary>
        /// <param name="categoryTemplateId">Category template identifier</param>
        /// <returns>Category template</returns>
        public virtual CategoryProductBoxTemplate GetCategoryTemplateById(int categoryTemplateId)
        {
            if (categoryTemplateId == 0)
                return null;

            return _categoryProductBoxTemplateRepository.GetById(categoryTemplateId);
        }
    }
}
