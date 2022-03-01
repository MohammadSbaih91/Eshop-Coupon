using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Customization.Models;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Customization.Validators.EShopCommon
{
    public partial class OrderTrackValidator : BaseNopValidator<OrderTrackModel>
    {
        public OrderTrackValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Address.Fields.Email.Required"));
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("AnonymousOrderTrack.WrongOrderId"));
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage(localizationService.GetResource("AnonymousOrderTrack.WrongOrderId"));
        }
    }
}