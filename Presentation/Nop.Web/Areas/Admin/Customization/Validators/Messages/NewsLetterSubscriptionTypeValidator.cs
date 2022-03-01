using FluentValidation;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Messages
{
    public partial class NewsLetterSubscriptionTypeValidator : BaseNopValidator<NewsLetterSubscriptionTypeModel>
    {
        public NewsLetterSubscriptionTypeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Messages.NewsLetterSubscriptionType.Fields.Name.Required"));
            RuleFor(x => x.Name).MaximumLength(400).WithMessage(localizationService.GetResource("Admin.Messages.NewsLetterSubscriptionType.Fields.Name.MaxLength"));
        }
    }
}
