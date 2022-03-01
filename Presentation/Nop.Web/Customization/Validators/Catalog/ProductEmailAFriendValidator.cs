using FluentValidation;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;

namespace Nop.Web.Validators.Catalog
{
    public partial class ProductEmailAFriendValidator 
    {
        protected override void PostInitialize()
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            RuleFor(x => x.FullName).NotEmpty().WithMessage(localizationService.GetResource("Products.EmailAFriend.FullName.Required"));
            base.PostInitialize();
        }
    }
}