using Nop.Web.Areas.Admin.Models.Card;
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Card
{
    public partial class SimCardValidator : BaseNopValidator<SimCardModel>
    {
        public SimCardValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.CardNumber).NotEmpty().WithMessage(localizationService.GetResource("Admin.SimCard.Fields.CardNumber.Required"));
        }
    }
}
