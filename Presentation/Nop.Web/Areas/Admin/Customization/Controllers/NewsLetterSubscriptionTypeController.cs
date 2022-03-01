using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Messages;
using Nop.Services.Messages;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class NewsLetterSubscriptionTypeController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IPermissionService _permissionService;
        private readonly INewsLetterSubscriptionTypeModelFactory _newsLetterSubscriptionTypeModelFactory;
        private readonly INewsLetterSubscriptionTypeService _newsLetterSubscriptionTypeService;

        #endregion Fields

        #region Ctor

        public NewsLetterSubscriptionTypeController(ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IPermissionService permissionService,
            INewsLetterSubscriptionTypeModelFactory newsLetterSubscriptionTypeModelFactory,
            INewsLetterSubscriptionTypeService newsLetterSubscriptionTypeService)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _permissionService = permissionService;
            _newsLetterSubscriptionTypeModelFactory = newsLetterSubscriptionTypeModelFactory;
            _newsLetterSubscriptionTypeService = newsLetterSubscriptionTypeService;
        }

        #endregion

        #region Utilities

        protected virtual void UpdateAttributeGroupLocales(NewsLetterSubscriptionType newsLetterSubscriptionType, NewsLetterSubscriptionTypeModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(newsLetterSubscriptionType,
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            //prepare model
            var model = _newsLetterSubscriptionTypeModelFactory.PrepareNewsLetterSubscriptionTypeSearchModel(new NewsLetterSubscriptionTypeSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(NewsLetterSubscriptionTypeSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _newsLetterSubscriptionTypeModelFactory.PrepareNewsLetterSubscriptionTypeListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            //prepare model
            var model = _newsLetterSubscriptionTypeModelFactory.PrepareNewsLetterSubscriptionTypeModel(new NewsLetterSubscriptionTypeModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(NewsLetterSubscriptionTypeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var newsLetterSubscriptionType = new NewsLetterSubscriptionType()
                {
                    Name = model.Name,
                    DisplayOrder = model.DisplayOrder
                };

                _newsLetterSubscriptionTypeService.InsertNewsLetterSubscriptionType(newsLetterSubscriptionType);
                UpdateAttributeGroupLocales(newsLetterSubscriptionType, model);

                //activity log
                _customerActivityService.InsertActivity("AddNewNewsLetterSubscriptionType",
                    string.Format(_localizationService.GetResource("ActivityLog.newsLetterSubscriptionType"), newsLetterSubscriptionType.Name), newsLetterSubscriptionType);

                SuccessNotification(_localizationService.GetResource("Admin.Messages.NewsLetterSubscriptionType.Fields.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = newsLetterSubscriptionType.Id });
            }

            //prepare model
            model = _newsLetterSubscriptionTypeModelFactory.PrepareNewsLetterSubscriptionTypeModel(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            //try to get a newsLetterSubscriptionType with the id
            var newsLetterSubscriptionType = _newsLetterSubscriptionTypeService.GetNewsLetterSubscriptionTypeById(id);
            if (newsLetterSubscriptionType == null)
                return RedirectToAction("List");

            //prepare model
            var model = _newsLetterSubscriptionTypeModelFactory.PrepareNewsLetterSubscriptionTypeModel(null, newsLetterSubscriptionType);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(NewsLetterSubscriptionTypeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            //try to get a newsLetterSubscriptionType with the id
            var newsLetterSubscriptionType = _newsLetterSubscriptionTypeService.GetNewsLetterSubscriptionTypeById(model.Id);
            if (newsLetterSubscriptionType == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                newsLetterSubscriptionType.Name = model.Name;
                newsLetterSubscriptionType.DisplayOrder = model.DisplayOrder;
                _newsLetterSubscriptionTypeService.UpdateNewsLetterSubscriptionType(newsLetterSubscriptionType);

                UpdateAttributeGroupLocales(newsLetterSubscriptionType, model);

                //activity log
                _customerActivityService.InsertActivity("EditnewsLetterSubscriptionType",
                    string.Format(_localizationService.GetResource("ActivityLog.newsLetterSubscriptionType"), newsLetterSubscriptionType.Name), newsLetterSubscriptionType);

                SuccessNotification(_localizationService.GetResource("Admin.Messages.NewsLetterSubscriptionType.Fields.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = newsLetterSubscriptionType.Id });
            }

            //prepare model
            model = _newsLetterSubscriptionTypeModelFactory.PrepareNewsLetterSubscriptionTypeModel(model, newsLetterSubscriptionType, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            //try to get a newsLetterSubscriptionType with the id
            var newsLetterSubscriptionType = _newsLetterSubscriptionTypeService.GetNewsLetterSubscriptionTypeById(id);
            if (newsLetterSubscriptionType == null)
                return RedirectToAction("List");

            _newsLetterSubscriptionTypeService.DeleteNewsLetterSubscriptionType(newsLetterSubscriptionType);

            //activity log
            _customerActivityService.InsertActivity("DeletenewsLetterSubscriptionType",
                string.Format(_localizationService.GetResource("ActivityLog.newsLetterSubscriptionType"), newsLetterSubscriptionType.Name), newsLetterSubscriptionType);

            SuccessNotification(_localizationService.GetResource("Admin.Messages.NewsLetterSubscriptionType.Fields.Deleted"));

            return RedirectToAction("List");
        }

        #endregion
    }
}