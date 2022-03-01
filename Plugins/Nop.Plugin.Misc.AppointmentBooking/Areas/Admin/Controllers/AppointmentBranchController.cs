using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Controllers;
using System.Linq;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Extensions;
using Nop.Plugin.Misc.AppointmentBooking.Services;
using Nop.Plugin.Misc.AppointmentBooking.Areas.Admin.Factories;
using Nop.Plugin.Misc.AppointmentBooking.Domains;
using Nop.Plugin.Misc.AppointmentBooking.Areas.Admin.Models;

namespace Nop.Plugin.Misc.AppointmentBooking.Areas.Admin.Controllers
{
    public class AppointmentBranchController : BaseAdminController
    {
        #region Fields
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IPermissionService _permissionService;
        private readonly IAppointmentService _appointmentService;
        private readonly IAppointmentBranchModelFactory _appointmentBranchModelFactory;
        #endregion

        #region Ctor
        public AppointmentBranchController(ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IPermissionService permissionService,
            IAppointmentService appointmentService,
            IAppointmentBranchModelFactory appointmentBranchModelFactory)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _permissionService = permissionService;
            _appointmentService = appointmentService;
            _appointmentBranchModelFactory = appointmentBranchModelFactory;
        }
        #endregion

        #region Utilities
        protected virtual void UpdateLocales(AppointmentBranch ab, AppointmentBranchModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(ab,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            }
        }
        #endregion

        #region Methods
        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //prepare model
            var model = new AppointmentBranchSearchModel();

            //prepare page parameters
            model.SetGridPageSize();

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(AppointmentBranchSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedKendoGridJson();

            //get appoint branch
            var appointmentBranches = _appointmentService.GetAppointmentBranchByName(searchModel.Name);

            //prepare list model
            var model = new AppointmentBranchListModel
            {
                Data = appointmentBranches.PaginationByRequestModel(searchModel).Select(appointmentBranche =>
                {
                    //fill in model values from the entity
                    var appointmentBrancheModel = new AppointmentBranchModel()
                    {
                        Id = appointmentBranche.Id,
                        BranchId = appointmentBranche.BranchId,
                        Name = appointmentBranche.Name,
                        Longitude = appointmentBranche.Longitude,
                        Latitude = appointmentBranche.Latitude
                    };

                    return appointmentBrancheModel;
                }),
                Total = appointmentBranches.Count
            };

            return Json(model);
        }
        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //prepare model
            var model = _appointmentBranchModelFactory.PrepareAppointmentBranchModel(new AppointmentBranchModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult Create(AppointmentBranchModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var appointmentBranch = new AppointmentBranch
                {
                    BranchId = model.BranchId,
                    Name = model.Name,
                    Longitude = model.Longitude,
                    Latitude = model.Latitude
                };

                _appointmentService.InsertAppointmentBranch(appointmentBranch);

                //activity log
                _customerActivityService.InsertActivity("CreateAppointmentBranch",
                    string.Format(_localizationService.GetResource("ActivityLog.CreateAppointmentBranch"), appointmentBranch.Id), appointmentBranch);

                //locales
                UpdateLocales(appointmentBranch, model);

                SuccessNotification(_localizationService.GetResource("Plugins.Misc.AppointmentBooking.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = appointmentBranch.Id });
            }

            //prepare model
            model = _appointmentBranchModelFactory.PrepareAppointmentBranchModel(model, null);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a appointment branch with the specified id
            var appointmentBranch = _appointmentService.GetAppointmentBranchById(id);
            if (appointmentBranch == null)
                return RedirectToAction("List");

            //prepare model
            var model = _appointmentBranchModelFactory.PrepareAppointmentBranchModel(null, appointmentBranch);


            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult Edit(AppointmentBranchModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a appointment branch with the specified id
            var appointmentBranch = _appointmentService.GetAppointmentBranchById(model.Id);
            if (appointmentBranch == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                appointmentBranch.BranchId = model.BranchId;
                appointmentBranch.Name = model.Name;
                appointmentBranch.Longitude = model.Longitude;
                appointmentBranch.Latitude = model.Latitude;

                _appointmentService.UpdateAppointmentBranch(appointmentBranch);

                //activity log
                _customerActivityService.InsertActivity("EditAppointmentBranch",
                    string.Format(_localizationService.GetResource("ActivityLog.EditAppointmentBranch"), appointmentBranch.Id), appointmentBranch);

                //locales
                UpdateLocales(appointmentBranch, model);

                SuccessNotification(_localizationService.GetResource("Plugins.Misc.AppointmentBooking.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = appointmentBranch.Id });
            }

            //prepare model
            model = _appointmentBranchModelFactory.PrepareAppointmentBranchModel(model, appointmentBranch);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a appointment branch with the specified id
            var appointmentBranch = _appointmentService.GetAppointmentBranchById(id);
            if (appointmentBranch == null)
                return RedirectToAction("List");

            _appointmentService.DeleteAppointmentBranch(appointmentBranch);

            //activity log
            _customerActivityService.InsertActivity("DeleteAppointmentBranch",
                string.Format(_localizationService.GetResource("ActivityLog.DeleteAppointmentBranch"), appointmentBranch.Name), appointmentBranch);

            SuccessNotification(_localizationService.GetResource("Plugins.Misc.AppointmentBooking.Deleted"));

            return RedirectToAction("List");
        }
        #endregion
    }
}
