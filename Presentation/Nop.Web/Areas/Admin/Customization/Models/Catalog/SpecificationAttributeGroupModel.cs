﻿using System.Collections.Generic;
using FluentValidation.Attributes;
using Nop.Web.Areas.Admin.Validators.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a specification attribute group model
    /// </summary>
    [Validator(typeof(SpecificationAttributeGroupValidator))]
    public partial class SpecificationAttributeGroupModel : BaseNopEntityModel, ILocalizedModel<SpecificationAttributeGroupLocalizedModel>
    {
        #region Ctor

        public SpecificationAttributeGroupModel()
        {
            Locales = new List<SpecificationAttributeGroupLocalizedModel>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributesGroup.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributesGroup.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<SpecificationAttributeGroupLocalizedModel> Locales { get; set; }
        
        #endregion
    }

    public partial class SpecificationAttributeGroupLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributesGroup.Fields.Name")]
        public string Name { get; set; }
    }
}
