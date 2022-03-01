using FluentValidation.Attributes;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Validators.Employees;

namespace Nop.Web.Models.Employees
{
    [Validator(typeof(EmployeeValidator))]
    public class EmployeeModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Employee.EmployeeName")]
        public string EmployeeName { get; set; }

        [NopResourceDisplayName("Employee.EmployeeId")]
        public string EmployeeId { get; set; }

        [NopResourceDisplayName("Employee.EmployeeContactNumber")]
        public string EmployeeContactNumber { get; set; }

        [NopResourceDisplayName("Employee.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Employee.Months")]
        public int Months { get; set; }

        [NopResourceDisplayName("Employee.Amount")]
        public decimal Amount { get; set; }

        [NopResourceDisplayName("Employee.OrderNumber")]
        public int OrderNumber { get; set; }

        public string Message { get; set; }
    }
}
