using FluentValidation.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Validators.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    [Validator(typeof(PackagesValidator))]
    public partial class PackagesModel : BaseNopEntityModel
    {
        public PackagesModel()
        {
            SelectedCategoryIds = new List<int>();
            AvailableCategories = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Admin.Packages.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Packages.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Packages.Fields.CategoryIds")]
        public string CategoryIds { get; set; }

        public bool Published { get; set; }
        [NopResourceDisplayName("Admin.Packages.Fields.CategoryIds")]
        public IList<int> SelectedCategoryIds { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }
    }
}
