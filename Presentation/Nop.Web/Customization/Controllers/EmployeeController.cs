using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Employees;
using Nop.Services.Employees;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Employees;
using System;

namespace Nop.Web.Controllers
{
    [HttpsRequirement(SslRequirement.Yes)]
    public class EmployeeController : BasePublicController
    {
        #region Fields
        private readonly IEmployeeService _employeeService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor
        public EmployeeController(IEmployeeService employeeService,
            ILocalizationService localizationService,
            IOrderService orderService,
            IWorkflowMessageService workflowMessageService,
            IWorkContext workContext)
        {
            _employeeService = employeeService;
            _localizationService = localizationService;
            _orderService = orderService;
            _workflowMessageService = workflowMessageService;
            _workContext = workContext;
        }
        #endregion

        #region Methods
        public virtual IActionResult EmployeeDetail(int orderId)
        {
            var employee = new EmployeeModel()
            {
                OrderNumber = orderId
            };
            return View(employee);
        }

        [HttpPost]
        public virtual IActionResult EmployeeDetail(EmployeeModel employeeModel)
        {
            if (_employeeService.IsEmployeeExistByOrderId(employeeModel.OrderNumber, employeeModel.Id))
            {
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("Admin.Employee.ExistWithOrder"), employeeModel.OrderNumber));
            }
            var order = _orderService.GetOrderById(employeeModel.OrderNumber);
            if (order == null)
            {
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("Admin.Employee.OrderNotExist"), employeeModel.OrderNumber));
            }

            if (ModelState.IsValid)
            {
                var employee = new Employee()
                {
                    EmployeeName = employeeModel.EmployeeName,
                    EmployeeId = employeeModel.EmployeeId,
                    EmployeeContactNumber = employeeModel.EmployeeContactNumber,
                    Email = employeeModel.Email,
                    Months = employeeModel.Months,
                    Amount = employeeModel.Amount,
                    OrderNumber = employeeModel.OrderNumber,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow
                };

                _employeeService.InsertEmployee(employee);

                _workflowMessageService.SendOrderEmployeeNotification(order, _workContext.WorkingLanguage.Id, employee);

                employeeModel.Message = string.Format(_localizationService.GetResource("Employee.EmployeeDetailAdded"), employeeModel.OrderNumber);
                return View(employeeModel);
            }
            return View(employeeModel);
        }
        #endregion
    }
}
