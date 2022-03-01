using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;

namespace Nop.Web.Areas.Admin.Factories
{
    public interface ISpecificationAttributeGroupModelFactory
    {
        SpecificationAttributeGroupSearchModel PrepareSpecificationAttributeGroupSearchModel(SpecificationAttributeGroupSearchModel searchModel);

        SpecificationAttributeGroupListModel PrepareSpecificationAttributeGroupListModel(SpecificationAttributeGroupSearchModel searchModel);

        SpecificationAttributeGroupModel PrepareSpecificationAttributeGroupModel(SpecificationAttributeGroupModel model,
            SpecificationAttributeGroup specificationAttributeGroup, bool excludeProperties = false);
    }

    public class SpecificationAttributeGroupModelFactory : ISpecificationAttributeGroupModelFactory
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ISpecificationAttributeGroupService _specificationAttributeGroupService;

        #endregion

        #region Ctor

        public SpecificationAttributeGroupModelFactory(ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            ISpecificationAttributeGroupService specificationAttributeGroupService)
        {
            this._localizationService = localizationService;
            this._localizedModelFactory = localizedModelFactory;
            this._specificationAttributeGroupService = specificationAttributeGroupService;
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Prepare specification attribute group search model
        /// </summary>
        /// <param name="searchModel">Specification attribute group search model</param>
        /// <returns>Specification attribute search model</returns>
        public virtual SpecificationAttributeGroupSearchModel PrepareSpecificationAttributeGroupSearchModel(SpecificationAttributeGroupSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged specification attribute group list model
        /// </summary>
        /// <param name="searchModel">Specification attribute search model</param>
        /// <returns>Specification attribute list model</returns>
        public virtual SpecificationAttributeGroupListModel PrepareSpecificationAttributeGroupListModel(SpecificationAttributeGroupSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get specification attribute groups
            var specificationAttributeGroups = _specificationAttributeGroupService
                .GetSpecificationAttributeGroups(searchModel.Page - 1, searchModel.PageSize);

            var spcGroupList = new List<SpecificationAttributeGroupModel>();
            foreach (var spcGroup in specificationAttributeGroups)
            {
                spcGroupList.Add(new SpecificationAttributeGroupModel
                {
                    Id = spcGroup.Id,
                    Name = _localizationService.GetLocalized(spcGroup, x => x.Name),
                    DisplayOrder = spcGroup.DisplayOrder
                });
            }

            //prepare list model
            var model = new SpecificationAttributeGroupListModel
            {
                //fill in model values from the entity
                Data = spcGroupList,
                Total = specificationAttributeGroups.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare specification attribut group model
        /// </summary>
        /// <param name="model">Specification attribute group model</param>
        /// <param name="specificationAttributeGroup">Specification attribute</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Specification attribute group model</returns>
        public virtual SpecificationAttributeGroupModel PrepareSpecificationAttributeGroupModel(SpecificationAttributeGroupModel model,
            SpecificationAttributeGroup specificationAttributeGroup, bool excludeProperties = false)
        {
            Action<SpecificationAttributeGroupLocalizedModel, int> localizedModelConfiguration = null;

            if (specificationAttributeGroup != null)
            {
                //fill in model values from the entity
                if (model != null)
                {
                    model.Id = specificationAttributeGroup.Id;
                    model.Name = specificationAttributeGroup.Name;
                    model.DisplayOrder = specificationAttributeGroup.DisplayOrder;
                }
                else
                {
                    model = new SpecificationAttributeGroupModel()
                    {
                        Id = specificationAttributeGroup.Id,
                        Name = specificationAttributeGroup.Name,
                        DisplayOrder = specificationAttributeGroup.DisplayOrder
                    };
                }

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(specificationAttributeGroup, entity => entity.Name, languageId, false, false);
                };
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }
        #endregion
    }
}
