using Nop.Core.Domain.Localization;
using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Web.Models.Catalog
{
    public class SpecificationAttributeGroupWithSpecId : BaseNopModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public int SpecificationId { get; set; }
    }

    public class CompareSpecificationAttributeGroupModel : BaseNopModel
    {
        public CompareSpecificationAttributeGroupModel()
        {
            SpecificationIds = new List<int>();
            SpecificationAttributeModels = new List<ProductSpecificationModel>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public IList<int> SpecificationIds { get; set; }
        public IList<ProductSpecificationModel> SpecificationAttributeModels { get; set; }
    }
}
