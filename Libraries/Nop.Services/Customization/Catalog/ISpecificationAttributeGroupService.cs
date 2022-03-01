using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    public interface ISpecificationAttributeGroupService
    {
        SpecificationAttributeGroup GetSpecificationAttributeGroupById(int SpecificationAttributeGroupId);

        IPagedList<SpecificationAttributeGroup> GetSpecificationAttributeGroups(int pageIndex = 0, int pageSize = int.MaxValue);

        void DeleteSpecificationAttributeGroup(SpecificationAttributeGroup specificationAttributeGroup);

        void InsertSpecificationAttributeGroup(SpecificationAttributeGroup specificationAttributeGroup);

        void UpdateSpecificationAttributeGroup(SpecificationAttributeGroup specificationAttributeGroup);
    }
}
