using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class SpecificationAttributeGroupController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IPermissionService _permissionService;
        private readonly ISpecificationAttributeGroupModelFactory _specificationAttributeGroupModelFactory;
        private readonly ISpecificationAttributeGroupService _specificationAttributeGroupService;

        #endregion Fields

        #region Ctor

        public SpecificationAttributeGroupController(ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IPermissionService permissionService,
            ISpecificationAttributeGroupModelFactory specificationAttributeGroupModelFactory,
            ISpecificationAttributeGroupService specificationAttributeGroupService)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _permissionService = permissionService;
            _specificationAttributeGroupModelFactory = specificationAttributeGroupModelFactory;
            _specificationAttributeGroupService = specificationAttributeGroupService;
        }

        #endregion

        #region Utilities

        protected virtual void UpdateAttributeGroupLocales(SpecificationAttributeGroup specificationAttributeGroup, SpecificationAttributeGroupModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(specificationAttributeGroup,
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //prepare model
            var model = _specificationAttributeGroupModelFactory.PrepareSpecificationAttributeGroupSearchModel(new SpecificationAttributeGroupSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(SpecificationAttributeGroupSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _specificationAttributeGroupModelFactory.PrepareSpecificationAttributeGroupListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //prepare model
            var model = _specificationAttributeGroupModelFactory.PrepareSpecificationAttributeGroupModel(new SpecificationAttributeGroupModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(SpecificationAttributeGroupModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //var specificationAttributeGoup = model.ToEntity<SpecificationAttributeGroup>();

                var specificationAttributeGoup = new SpecificationAttributeGroup()
                {
                    Name = model.Name,
                    DisplayOrder = model.DisplayOrder
                };

                _specificationAttributeGroupService.InsertSpecificationAttributeGroup(specificationAttributeGoup);
                UpdateAttributeGroupLocales(specificationAttributeGoup, model);

                //activity log
                _customerActivityService.InsertActivity("AddNewSpecAttributeGroup",
                    string.Format(_localizationService.GetResource("ActivityLog.AddNewSpecAttributeGroup"), specificationAttributeGoup.Name), specificationAttributeGoup);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.SpecificationAttributesGroup.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = specificationAttributeGoup.Id });
            }

            //prepare model
            model = _specificationAttributeGroupModelFactory.PrepareSpecificationAttributeGroupModel(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //try to get a specification attribute group with the specified id
            var specificationAttributeGroup = _specificationAttributeGroupService.GetSpecificationAttributeGroupById(id);
            if (specificationAttributeGroup == null)
                return RedirectToAction("List");

            //prepare model
            var model = _specificationAttributeGroupModelFactory.PrepareSpecificationAttributeGroupModel(null, specificationAttributeGroup);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(SpecificationAttributeGroupModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //try to get a specification attribute group with the specified id
            var specificationAttributeGroup = _specificationAttributeGroupService.GetSpecificationAttributeGroupById(model.Id);
            if (specificationAttributeGroup == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                specificationAttributeGroup.Name = model.Name;
                specificationAttributeGroup.DisplayOrder = model.DisplayOrder;
                _specificationAttributeGroupService.UpdateSpecificationAttributeGroup(specificationAttributeGroup);

                UpdateAttributeGroupLocales(specificationAttributeGroup, model);

                //activity log
                _customerActivityService.InsertActivity("EditSpecAttributeGroup",
                    string.Format(_localizationService.GetResource("ActivityLog.EditSpecAttributeGroup"), specificationAttributeGroup.Name), specificationAttributeGroup);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.SpecificationAttributesGroup.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = specificationAttributeGroup.Id });
            }

            //prepare model
            model = _specificationAttributeGroupModelFactory.PrepareSpecificationAttributeGroupModel(model, specificationAttributeGroup, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //try to get a specification attribute group with the specified id
            var specificationAttributeGroup = _specificationAttributeGroupService.GetSpecificationAttributeGroupById(id);
            if (specificationAttributeGroup == null)
                return RedirectToAction("List");

            _specificationAttributeGroupService.DeleteSpecificationAttributeGroup(specificationAttributeGroup);

            //activity log
            _customerActivityService.InsertActivity("DeleteSpecAttributeGroup",
                string.Format(_localizationService.GetResource("ActivityLog.DeleteSpecAttributeGroup"), specificationAttributeGroup.Name), specificationAttributeGroup);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.SpecificationAttributesGroup.Deleted"));

            return RedirectToAction("List");
        }

        #endregion
    }
}