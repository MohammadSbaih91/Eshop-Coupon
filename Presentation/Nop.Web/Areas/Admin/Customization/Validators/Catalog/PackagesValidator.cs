using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Validators.Catalog
{
    public partial class PackagesValidator : BaseNopValidator<PackagesModel>
    {
        public PackagesValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Packages.Fields.Name.Required"));
            RuleFor(x => x.DisplayOrder).NotEmpty().WithMessage(localizationService.GetResource("Admin.Packages.Fields.DisplayOrder.Required"));
        }
    }
}
