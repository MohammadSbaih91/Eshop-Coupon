using Nop.Web.Models.Catalog;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Localization;

namespace Nop.Web.Factories
{
    public interface ISpecificationAttributeGroupModelFactory
    {
        List<CompareSpecificationAttributeGroupModel> getSpecificationAttributeWithSpecAttrib();
    }

    public class SpecificationAttributeGroupModelFactory : ISpecificationAttributeGroupModelFactory
    {
        private readonly IRepository<SpecificationAttributeGroup> _specificationAttributeGroupRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributerepository;
        private readonly ILocalizationService _localizationService;

        public SpecificationAttributeGroupModelFactory(IRepository<SpecificationAttributeGroup> specificationAttributeGroupRepository,
            IRepository<SpecificationAttribute> specificationAttributerepository,
            ILocalizationService localizationService)
        {
            _specificationAttributeGroupRepository = specificationAttributeGroupRepository;
            _specificationAttributerepository = specificationAttributerepository;
            _localizationService = localizationService;
        }

        public List<CompareSpecificationAttributeGroupModel> getSpecificationAttributeWithSpecAttrib()
        {
            var saQuery = _specificationAttributeGroupRepository.Table.ToList();

            var saGroup = new List<SpecificationAttributeGroup>();
            foreach (var sag in saQuery)
            {
                saGroup.Add(new SpecificationAttributeGroup()
                {
                    Id = sag.Id,
                    Name = _localizationService.GetLocalized(sag, x => x.Name),
                    DisplayOrder = sag.DisplayOrder,
                });
            }

            var query = (from sag in saGroup
                         join sa in _specificationAttributerepository.Table on sag.Id equals sa.SpecificationAttributeGroupId
                        select(new SpecificationAttributeGroupWithSpecId() {
                            Id = sag.Id,
                            Name = _localizationService.GetLocalized(sag, x => x.Name),
                            DisplayOrder = sag.DisplayOrder,
                            SpecificationId = sa.Id
                        }));

            
            var grouplist =
                    from c in query
                    group c by new
                    {
                        c.Id,
                        c.Name,
                        c.DisplayOrder,
                    } into gcs
                    select new CompareSpecificationAttributeGroupModel()
                    {
                        Id = gcs.Key.Id,
                        Name = gcs.Key.Name,
                        DisplayOrder = gcs.Key.DisplayOrder,
                        SpecificationIds = query.Where(p =>p.Id == gcs.Key.Id).Select(p => p.SpecificationId).ToList()
                    };


            var grplist = grouplist.ToList();
            grplist.Add(new CompareSpecificationAttributeGroupModel()
             {
                 Id = -1,
                 Name = _localizationService.GetResource("Admin.Catalog.Attributes.SpecificationAttributes.SpecificationAttributeGroup.Other"),
                 DisplayOrder = int.MinValue
             });

            return grplist.OrderBy(p=>p.DisplayOrder).ToList();
        }
    }
}
