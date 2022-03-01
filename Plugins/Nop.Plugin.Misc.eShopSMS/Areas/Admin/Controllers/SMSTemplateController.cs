using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Controllers;
using System.Linq;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.Misc.eShopSMS.Services;
using Nop.Web.Framework.Extensions;
using Nop.Plugin.Misc.eShopSMS.Areas.Admin.Models;
using Nop.Plugin.Misc.eShopSMS.Areas.Admin.Factories;
using Nop.Plugin.Misc.eShopSMS.Domains;

namespace Nop.Plugin.Misc.eShopSMS.Areas.Admin.Controllers
{
    public class SMSTemplateController : BaseAdminController
    {
        #region Fields
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IPermissionService _permissionService;
        private readonly ISmsTemplateService _smsTemplateService;
        private readonly ISmsTemplateModelFactory _smsTemplateModelFactory;
        #endregion

        #region Ctor
        public SMSTemplateController(ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            ISmsTemplateService smsTemplateService,
            IPermissionService permissionService,
            ISmsTemplateModelFactory smsTemplateModelFactory)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _smsTemplateService = smsTemplateService;
            _permissionService = permissionService;
            _smsTemplateModelFactory = smsTemplateModelFactory;
        }
        #endregion

        #region Utilities
        protected virtual void UpdateLocales(SMSTemplate mt, SmsTemplateModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(mt,
                    x => x.Body,
                    localized.Body,
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
            var model = new SmsTemplateSearchModel();

            //prepare page parameters
            model.SetGridPageSize();

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(SmsTemplateSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedKendoGridJson();

            //get sms templates
            var smsTemplates = _smsTemplateService.GetAllSmsTemplates();
            
            //prepare list model
            var model = new SmsTemplateListModel
            {
                Data = smsTemplates.PaginationByRequestModel(searchModel).Select(smsTemplate =>
                {
                    //fill in model values from the entity
                    var smsTemplateModel = new SmsTemplateModel()
                    {
                        Id = smsTemplate.Id,
                        TemplateName = smsTemplate.TemplateName,
                        IsActive = smsTemplate.IsActive
                    };
                    
                    return smsTemplateModel;
                }),
                Total = smsTemplates.Count
            };

            return Json(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a sms template with the specified id
            var smsTemplate = _smsTemplateService.GetSmsTemplateById(id);
            if (smsTemplate == null)
                return RedirectToAction("List");

            //prepare model
            var model = _smsTemplateModelFactory.PrepareSmsTemplateModel(null, smsTemplate);


            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult Edit(SmsTemplateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a sms template with the specified id
            var smsTemplate = _smsTemplateService.GetSmsTemplateById(model.Id);
            if (smsTemplate == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                smsTemplate.Body = model.Body;
                smsTemplate.IsActive = model.IsActive;

                _smsTemplateService.UpdateSMSTemplate(smsTemplate);

                //activity log
                _customerActivityService.InsertActivity("EditSmsTemplate",
                    string.Format(_localizationService.GetResource("ActivityLog.EditSmsTemplate"), smsTemplate.Id), smsTemplate);
                
                //locales
                UpdateLocales(smsTemplate, model);

                SuccessNotification(_localizationService.GetResource("Plugins.Misc.eShopSMS.smsTemplates.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = smsTemplate.Id });
            }

            //prepare model
            model = _smsTemplateModelFactory.PrepareSmsTemplateModel(model, smsTemplate);

            //if we got this far, something failed, redisplay form
            return View(model);
        }
        #endregion
    }
}
