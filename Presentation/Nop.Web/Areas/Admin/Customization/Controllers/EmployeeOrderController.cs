using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Customization.Orders;
using Nop.Services.Employees;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.EmployeeOrder;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class EmployeeOrderController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IEmployeeOrderModelFactory _employeeOrderModelFactory;
        private readonly IEmployeeService _employeeService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor
        public EmployeeOrderController(IPermissionService permissionService,
            IEmployeeOrderModelFactory employeeOrderModelFactory,
            IEmployeeService employeeService,
            ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _employeeOrderModelFactory = employeeOrderModelFactory;
            _employeeService = employeeService;
            _localizationService = localizationService;
        }
        #endregion

        #region Methods
        public virtual IActionResult Index()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = _employeeOrderModelFactory.PrepareCustomReportsSearchModel(new EmployeeOrderSearchModel());
            
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult EmployeeOrderList(EmployeeOrderSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _employeeOrderModelFactory.PrepareOrderListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult EditEmployeeDetail(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            var employee = _employeeService.GetEmployeeByOrderId(orderId);
            if (employee == null)
                return RedirectToAction("List");
            
            var model = new Nop.Web.Models.Employees.EmployeeModel()
            {
                Id = employee.Id,
                EmployeeName = employee.EmployeeName,
                EmployeeId = employee.EmployeeId,
                EmployeeContactNumber = employee.EmployeeContactNumber,
                Email = employee.Email,
                Months = employee.Months,
                Amount = employee.Amount,
                OrderNumber = employee.OrderNumber
            };

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult EditEmployeeDetail(Nop.Web.Models.Employees.EmployeeModel employeeModel, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            //try to get a category with the specified id
            var employee = _employeeService.GetEmployeeById(employeeModel.Id);
            if (employee == null)
                return RedirectToAction("List");

            if (_employeeService.IsEmployeeExistByOrderId(employeeModel.OrderNumber, employeeModel.Id))
            {
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("Admin.Employee.ExistWithOrder"), employeeModel.OrderNumber));
            }

            if (ModelState.IsValid)
            {
                employee.EmployeeName = employeeModel.EmployeeName;
                employee.EmployeeId = employeeModel.EmployeeId;
                employee.EmployeeContactNumber = employeeModel.EmployeeContactNumber;
                employee.Email = employeeModel.Email;
                employee.Months = employeeModel.Months;
                employee.Amount = employeeModel.Amount;
                employee.UpdatedOn = DateTime.UtcNow;

                _employeeService.UpdateEmployee(employee);

                SuccessNotification(_localizationService.GetResource("Admin.Employee.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("EditEmployeeDetail", new { orderId = employee.OrderNumber });
            }

            return View(employeeModel);
        }
        #endregion
    }
}
