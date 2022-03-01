using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public class PackageProductSearchModel : BaseSearchModel
    {
        public int PackageId { get; set; }
    }
}
