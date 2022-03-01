using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial class SpecificationAttributeModel
    {
        [NopResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.SpecificationAttributeGroup")]
        public int SpecificationAttributeGroupId { get; set; }
        public IList<SelectListItem> AvailableSpecificationAttributeGroup { get; set; } = new List<SelectListItem>();

        [NopResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.PictureId")]
        [UIHint("Picture")]
        public int PictureId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.IsShowInsideBox")]
        public bool IsShowInsideBox { get; set; }
    }
}
