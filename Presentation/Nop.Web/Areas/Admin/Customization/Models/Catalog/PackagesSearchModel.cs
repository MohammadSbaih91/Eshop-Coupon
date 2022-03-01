using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial class PackagesSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.Packages.Fields.Name")]
        public string Name { get; set; }
    }
}
