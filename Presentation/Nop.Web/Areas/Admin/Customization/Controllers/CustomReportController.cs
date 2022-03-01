using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Customization.Orders;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Customization.Factories;
using Nop.Web.Areas.Admin.Customization.Models.Report;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class CustomReportController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ICustomReportModelFactory _customReportModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomeOrderService _customeOrderService;
        private readonly IExportManager _exportManager;
        #endregion

        #region Ctor
        public CustomReportController(IPermissionService permissionService,
            ICustomReportModelFactory customReportModelFactory,
            IWorkContext workContext,
            IProductService productService,
            IDateTimeHelper dateTimeHelper,
            ICustomeOrderService customeOrderService,
            IExportManager exportManager)
        {
            _permissionService = permissionService;
            _customReportModelFactory = customReportModelFactory;
            _workContext = workContext;
            _productService = productService;
            _dateTimeHelper = dateTimeHelper;
            _customeOrderService = customeOrderService;
            _exportManager = exportManager;
        }
        #endregion

        #region Methods
        public virtual IActionResult CustomizeReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //prepare model
            var model = _customReportModelFactory.PrepareCustomReportsSearchModel(new CustomReportSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult CustomizeReport(CustomReportSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = _customReportModelFactory.PrepareCustomReportListModel(searchModel);
            
            return Json(model);
        }

        [HttpPost, ActionName("CustomizeReport")]
        [FormValueRequired("exportexcel-all")]
        public virtual IActionResult ExportExcelAll(CustomReportSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var startDateValue = !searchModel.StartDate.HasValue ? null
               : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, _dateTimeHelper.CurrentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);
            var product = _productService.GetProductById(searchModel.ProductId);
            var filterByProductId = product != null && (_workContext.CurrentVendor == null || product.VendorId == _workContext.CurrentVendor.Id)
                ? searchModel.ProductId : 0;

            var reportData = _customReportModelFactory.PrepareCustomReportModel(out int totalCount,
                 startDateValue,
                 endDateValue,
                 searchModel.OrderStatusId,
                 filterByProductId);

            //var reportData = _customeOrderService.SearchExportOrderReport(startDateValue,
            //    endDateValue,
            //    searchModel.OrderStatusId,
            //    filterByProductId);

            try
            {
                var bytes = _customReportModelFactory.ExportCustomizeReportToXlsx(reportData);
                return File(bytes, MimeTypes.TextXlsx, "report.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("CustomizeReport");
            }
        }

        public virtual IActionResult ProductSearchAutoComplete(string term)
        {
            const int searchTermMinimumLength = 3;
            if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content(string.Empty);

            //a vendor should have access only to his products
            var vendorId = 0;
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            //products
            const int productNumber = 15;
            var products = _productService.SearchProducts(
                vendorId: vendorId,
                keywords: term,
                pageSize: productNumber,
                showHidden: true);

            var result = (from p in products
                          select new
                          {
                              label = p.Name,
                              productid = p.Id
                          }).ToList();
            return Json(result);
        }
        #endregion
    }
}
