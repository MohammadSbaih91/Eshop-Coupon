using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Customization.Catalog;
using Nop.Services.Localization;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial interface ICustomeBaseAdminModelFactory
    {
        void PrepareCategoryProductBoxTemplate(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);
    }

    public partial class CustomeBaseAdminModelFactory : ICustomeBaseAdminModelFactory
    {
        private readonly ICategoryProductBoxTemplateService _categoryProductBoxTemplateService;
        private readonly ILocalizationService _localizationService;

        public CustomeBaseAdminModelFactory(ICategoryProductBoxTemplateService categoryProductBoxTemplateService,
            ILocalizationService localizationService)
        {
            _categoryProductBoxTemplateService = categoryProductBoxTemplateService;
            _localizationService = localizationService;
        }

        #region Utilities

        /// <summary>
        /// Prepare default item
        /// </summary>
        /// <param name="items">Available items</param>
        /// <param name="withSpecialDefaultItem">Whether to insert the first special item for the default value</param>
        /// <param name="defaultItemText">Default item text; pass null to use "All" text</param>
        protected virtual void PrepareDefaultItem(IList<SelectListItem> items, bool withSpecialDefaultItem, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //whether to insert the first special item for the default value
            if (!withSpecialDefaultItem)
                return;

            //at now we use "0" as the default value
            const string value = "0";

            //prepare item text
            defaultItemText = defaultItemText ?? _localizationService.GetResource("Admin.Common.All");

            //insert this default item at first
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = value });
        }

        #endregion

        /// <summary>
        /// Prepare available category templates
        /// </summary>
        /// <param name="items">Category template items</param>
        /// <param name="withSpecialDefaultItem">Whether to insert the first special item for the default value</param>
        /// <param name="defaultItemText">Default item text; pass null to use default value of the default item text</param>
        public virtual void PrepareCategoryProductBoxTemplate(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //prepare available category templates
            var availableTemplates = _categoryProductBoxTemplateService.GetAllCategoryTemplates();
            foreach (var template in availableTemplates)
            {
                items.Add(new SelectListItem { Value = template.Id.ToString(), Text = template.Name });
            }

            //insert special item for the default value
            PrepareDefaultItem(items, withSpecialDefaultItem, defaultItemText);
        }
    }
}
