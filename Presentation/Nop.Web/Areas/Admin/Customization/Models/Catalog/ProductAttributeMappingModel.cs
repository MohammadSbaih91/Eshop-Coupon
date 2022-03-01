using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductAttributeMappingModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Catalog.Attributes.AttributesMap.Fields.IsShowButton")]
        public bool IsShowButton { get; set; }
    }
}
