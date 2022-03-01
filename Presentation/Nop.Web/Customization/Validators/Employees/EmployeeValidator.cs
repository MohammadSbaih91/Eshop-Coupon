using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Employees;

namespace Nop.Web.Validators.Employees
{
    public class EmployeeValidator : BaseNopValidator<EmployeeModel>
    {
        public EmployeeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.EmployeeName)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Employee.EmployeeName.Required"));

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Employee.Email.Required"));
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage(localizationService.GetResource("Employee.Email.WrongEmail"));

            RuleFor(x => x.EmployeeContactNumber).
                Must(x => x.StartsWith("077"))
                .WithMessage(localizationService.GetResource("Employee.EmployeeContactNumber.NotValied"));

            RuleFor(x => x.Months).LessThanOrEqualTo(18)
                .WithMessage(localizationService.GetResource("Employee.Months.NotValied"));

            RuleFor(x => x.Amount).GreaterThanOrEqualTo(20)
                .WithMessage(localizationService.GetResource("Employee.Amount.NotValied"));
        }
    }
}
