using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Core;
using Nop.Core.Domain.Vendors;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using ProductImportNUpdate.Services;

namespace ProductImportNUpdate.Controllers
{
    [AuthorizeAdmin, Area(AreaNames.Admin)]
    public class ProductImportNUpdateController : BasePluginController
    {
        private readonly IPermissionService _permissionService;

        private readonly IImportService _importService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly VendorSettings _vendorSettings;

        public ProductImportNUpdateController(IPermissionService permissionService,
            ILocalizationService localizationService,
            IWorkContext workContext, VendorSettings vendorSettings, IImportService importService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _workContext = workContext;
            _vendorSettings = vendorSettings;
            _importService = importService;
        }

        public IActionResult Configure() => !_permissionService.Authorize(StandardPermissionProvider.ManageWidgets)
            ? AccessDeniedView()
            : View("~/Plugins/ProductImportNUpdate/Views/Configure.cshtml");


        [HttpPost]
        [AdminAntiForgery]
        public virtual IActionResult Configure(IFormFile importFile)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (_workContext.CurrentVendor != null && !_vendorSettings.AllowVendorsToImportProducts)
                //a vendor can not import products
                return AccessDeniedView();

            try
            {
                if (importFile == null || importFile.Length <= 0)
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return Configure();
                }

                if (!importFile.FileName.EndsWith(".csv",StringComparison.InvariantCultureIgnoreCase))
                    throw new UnsupportedContentTypeException(_localizationService.GetResource("Plugin.ProductImportNUpdate.UnsupportedFile"));

                var result = _importService.ImportProductsFromStream(importFile.OpenReadStream());
                if (result.IsError)
                    WarningNotification(result.ToString());
                else
                    SuccessNotification(result.ToString());
                
                return Configure();
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return Configure();
            }
        }
    }
}