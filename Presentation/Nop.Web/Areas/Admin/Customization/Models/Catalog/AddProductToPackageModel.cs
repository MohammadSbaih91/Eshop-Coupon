using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial class AddProductToPackageModel : BaseNopModel
    {
        #region Ctor

        public AddProductToPackageModel()
        {
            SelectedProductIds = new List<int>();
        }
        #endregion

        #region Properties

        public int PackageId { get; set; }

        public IList<int> SelectedProductIds { get; set; }

        #endregion
    }
}
