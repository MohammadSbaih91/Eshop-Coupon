using System;
using System.Text.RegularExpressions;
using FluentValidation;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;

namespace Nop.Web.Validators.Common
{
    public partial class AddressValidator  
    {
        protected override void PostInitialize()
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
           
            RuleFor(x => x.Nationality)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Address.Fields.Nationality.Required"));
            
            RuleFor(x => x.NationalityType)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Address.Fields.NationalityType.Required"));
            
            RuleFor(x => x.Civility)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Address.Fields.Civility.Required"));
            
            RuleFor(x => x.IdentityCardOrPassport)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Address.Fields.IdentityCardOrPassport.Required"));
            
            RuleFor(x => x.EmailConfirm)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Address.Fields.EmailConfirm.Required"));
            RuleFor(x => x.EmailConfirm)
                .EmailAddress()
                .WithMessage(localizationService.GetResource("Common.WrongEmail"));
           
            RuleFor(x => x.EmailConfirm).Must((model,v) => string.Equals(v,model.Email,StringComparison.InvariantCultureIgnoreCase))   
                .WithMessage(localizationService.GetResource("Address.Fields.EmailConfirm.MissMatch"));

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Address.Fields.PhoneNumber.Required"));


            RuleFor(x => x.StudentID)
                .NotEmpty().When(x => x.IsStudentIdNeeded = true)
                .WithMessage(localizationService.GetResource("Address.Fields.StudentID.Required"));

            RuleFor(x => x.UploadStudentID)
               .NotEmpty().When(x=>x.IsStudentIdNeeded=true)
               .WithMessage(localizationService.GetResource("Address.Fields.UploadStudentID.Required"));

            RuleFor(x => x.PhoneNumber).Must((model, ph) => {
                
                    var exp = @"\+?962[789]\d{8}";
                    if (Regex.IsMatch(model.PhoneNumber, exp, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
                        return true;
                    else
                        return false;
                }).WithMessage(localizationService.GetResource("Address.Fields.PhoneNumber.MissMatch"));

            base.PostInitialize();
        }
    }
}