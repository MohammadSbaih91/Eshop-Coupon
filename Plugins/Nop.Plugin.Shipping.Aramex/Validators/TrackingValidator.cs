using Nop.Plugin.Shipping.Aramex.Models;
using Nop.Web.Framework.Validators;
using Nop.Core.Domain.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using FluentValidation;

namespace Nop.Plugin.Shipping.Aramex.Validators
{
    public partial class TrackingValidator : BaseNopValidator<TrackingModel>
    {
        public TrackingValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.OrderNumber).NotEmpty().WithMessage(localizationService.GetResource("Shipping.Aramex.Tracking.OrderNumber.Required"));
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Shipping.Aramex.Tracking.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
        }
    }
}
