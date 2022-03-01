using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Customization.Models.Catalog;
using Nop.Web.Framework.Components;
using System.Linq;
using Nop.Services.Common;

namespace Nop.Web.Customization.Components
{
    public class CategoryFilterViewComponent : NopViewComponent
    {
        #region Fieds
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        #endregion

        #region Ctor
        public CategoryFilterViewComponent(ICategoryService categoryService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService)
        {
            this._categoryService = categoryService;
            this._localizationService = localizationService;
            this._urlRecordService = urlRecordService;
        }
        #endregion
        
        #region Method
        public IViewComponentResult Invoke(int currentCategoryId, int currentProductId)
        {
            var model = new CategoryFilterModel();
            var categories = _categoryService.GetAllCategories();
            var parentcatgory = categories.Where(p => p.Id == currentCategoryId).First();

            L1:
            if (parentcatgory.ParentCategoryId != 0)
            {
                parentcatgory = categories.Where(p => p.Id == parentcatgory.ParentCategoryId).First();
                goto L1;
            }
            var subCategory = categories.Where(p => p.ParentCategoryId == parentcatgory.Id).OrderBy(p=>p.DisplayOrder).ToList();
            foreach (var category in categories)
            {
                if (category.ParentCategoryId == 0)
                {
                    var subCat = categories.Where(p => p.ParentCategoryId == category.Id).OrderBy(p => p.DisplayOrder).FirstOrDefault();
                    model.Category.Add(new SelectListItem
                    {
                        Text = _localizationService.GetLocalized(category, x => x.Name),
                        Value = UrlStrucutre.UrlDecode(subCat != null ? _urlRecordService.GetSeName(subCat) : _urlRecordService.GetSeName(category)),
                        Selected = category.Id == parentcatgory.Id
                    });
                }
            }
            foreach (var category in subCategory)
            {
                model.SubCategory.Add(new SelectListItem
                {
                    Text = _localizationService.GetLocalized(category, x => x.Name),
                    Value = UrlStrucutre.UrlDecode(_urlRecordService.GetSeName(category)),
                    Selected = category.Id == currentCategoryId
                });
            }
            return View(model);
        }
        #endregion
    }
}
