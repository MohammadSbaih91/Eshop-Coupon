using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;

namespace Nop.Web.Areas.Admin.Factories
{
    public class OverrideSpecificationAttributeModelFactory : SpecificationAttributeModelFactory
    {
        #region Fields
        private readonly ISpecificationAttributeGroupService _specificationAttributeGroupService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public OverrideSpecificationAttributeModelFactory(ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            ISpecificationAttributeService specificationAttributeService,
            ISpecificationAttributeGroupService specificationAttributeGroupService) : base(localizationService,
            localizedModelFactory,
            specificationAttributeService)
        {
            _specificationAttributeGroupService = specificationAttributeGroupService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Prepare specification attribute model
        /// </summary>
        /// <param name="model">Specification attribute model</param>
        /// <param name="specificationAttribute">Specification attribute</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Specification attribute model</returns>
        public override SpecificationAttributeModel PrepareSpecificationAttributeModel(SpecificationAttributeModel model,
            SpecificationAttribute specificationAttribute, bool excludeProperties = false)
        {
            var preparemodel = base.PrepareSpecificationAttributeModel(model, specificationAttribute, excludeProperties);

            preparemodel.AvailableSpecificationAttributeGroup.Add(new SelectListItem()
            {
                Text = _localizationService.GetResource("Admin.Catalog.Attributes.SpecificationAttributes.Fields.SpecificationAttributeGroup.Default"),
                Value = "0"
            });

            var groups = _specificationAttributeGroupService.GetSpecificationAttributeGroups();
            foreach (var group in groups)
            {
                preparemodel.AvailableSpecificationAttributeGroup.Add(new SelectListItem()
                {
                    Text = _localizationService.GetLocalized(group, x => x.Name),
                    Value = group.Id.ToString(),
                    Selected = group.Id == preparemodel.SpecificationAttributeGroupId
                });
            }
            
            return preparemodel;
        }
        #endregion
    }
}
