using System;

namespace Nop.Core.Domain.Employees
{
    public class Employee : BaseEntity
    {
        public string EmployeeName { get; set; }

        public string EmployeeId { get; set; }

        public string EmployeeContactNumber { get; set; }

        public string Email { get; set; }

        public int Months { get; set; }

        public decimal Amount { get; set; }

        public int OrderNumber { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}