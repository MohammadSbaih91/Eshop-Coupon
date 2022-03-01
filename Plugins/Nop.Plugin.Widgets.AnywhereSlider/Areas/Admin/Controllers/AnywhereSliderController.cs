using Nop.Plugin.Widgets.AnywhereSlider.Services;
using Nop.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using Nop.Plugin.Widgets.AnywhereSlider.Areas.Admin.Factories;
using Nop.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models;
using Nop.Plugin.Widgets.AnywhereSlider.Domains;
using Nop.Web.Framework.Factories;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.AnywhereSlider.Areas.Admin.Controllers
{
    public class AnywhereSliderController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IAnywhereSliderService _anywhereSliderService;
        private readonly IAnywhereSliderFactory _anywhereSliderFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ILocalizedEntityService _localizedEntityService;
        #endregion

        #region Ctor
        public AnywhereSliderController(IAnywhereSliderService anywhereSliderService,
            IPermissionService permissionService,
            IAnywhereSliderFactory anywhereSliderFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            ILocalizedEntityService localizedEntityService)
        {
            _anywhereSliderService = anywhereSliderService;
            _permissionService = permissionService;
            _anywhereSliderFactory = anywhereSliderFactory;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _localizedEntityService = localizedEntityService;
        }
        #endregion

        #region Utilities
        protected virtual void UpdateLocales(Accordingly accordingly, AccordinglyModel model)
        {

            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(accordingly,
                    x => x.PictureId,
                    localized.PictureId,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(accordingly,
                x => x.MobilePictureId,
                localized.MobilePictureId,
                localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(accordingly,
                x => x.TabletPictureId,
                localized.TabletPictureId,
                localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(accordingly,
            x => x.Position,
            localized.Position,
            localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(accordingly,
            x => x.Alignment,
            localized.Alignment,
            localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(accordingly,
                    x => x.ClickToAction,
                    localized.ClickToAction,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(accordingly,
                    x => x.Html,
                    localized.Html,
                    localized.LanguageId);
            }
        }

        protected virtual void UpdateSliderGroupLocales(SliderGroup sliderGroup, SliderGroupModel model)
        {

            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(sliderGroup,
                    x => x.Title,
                    localized.Title,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(sliderGroup,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);
            }
        }

        protected virtual IList<SelectListItem> PreparePositions(int selected)
        {
            var defaultSelection = new SelectListItem()
            {
                Value = "",
                Text = "-Select-",
            };
            var positions = Enum.GetValues(typeof(PositionEnum)).Cast<PositionEnum>()
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = x.ToString(),
                    Selected = (int)x == selected
                }).ToList();


            positions.Insert(0, defaultSelection);

            return positions;
        }
        protected virtual IList<SelectListItem> PrepareAlignments(int selected)
        {
            var defaultSelection = new SelectListItem()
            {
                Value = "",
                Text = "-Select-",
            };
            var alignments = Enum.GetValues(typeof(AlignmentEnum)).Cast<AlignmentEnum>()
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = x.ToString(),
                    Selected = (int)x == selected
                }).ToList();

            alignments.Insert(0, defaultSelection);

            return alignments;
        }
        #endregion

        #region Methods
        #region List
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = new SliderSearchModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult List(SliderSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //Prepare cart model
            var model = _anywhereSliderFactory.PrepareSliderListModel(searchModel);

            return Json(model);
        }
        #endregion

        #region Create / Edit / Delete
        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _anywhereSliderFactory.PrepareSliderModel(null, null);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(SliderModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var slider = new Slider()
                {
                    Name = model.Name,
                    WidgetZone = model.WidgetZone,
                    Published = model.Published
                };

                _anywhereSliderService.InsertSlider(slider);

                SuccessNotification(_localizationService.GetResource("Widgets.AnywhereSlider.Slider.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = slider.Id });

            }

            //if we got this far, something failed, redisplay form
            model = _anywhereSliderFactory.PrepareSliderModel(null, model);
            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a slider with the specified id
            var slider = _anywhereSliderService.GetSliderById(id);
            if (slider == null)
                return RedirectToAction("List");

            //prepare model
            var model = _anywhereSliderFactory.PrepareSliderModel(slider, null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(SliderModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a nopAmalgamationGroup with the specified id
            var slider = _anywhereSliderService.GetSliderById(model.Id);
            if (slider == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                slider.Name = model.Name;
                slider.WidgetZone = model.WidgetZone;
                slider.Published = model.Published;

                _anywhereSliderService.UpdateSlider(slider);

                SuccessNotification(_localizationService.GetResource("Widgets.AnywhereSlider.Slider.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = slider.Id });
            }

            //prepare model
            //model = _amalgamationGroupModelFactory.PrepareNopAmalgamationGroupModel(model, amalgamationGroup);

            //if we got this far, something failed, redisplay form
            model = _anywhereSliderFactory.PrepareSliderModel(null, model);
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a amalgamationGroup with the specified id
            var slider = _anywhereSliderService.GetSliderById(id);
            if (slider == null)
                return RedirectToAction("List");

            _anywhereSliderService.DeleteSlider(slider);

            SuccessNotification(_localizationService.GetResource("Widgets.AnywhereSlider.Slider.Deleted"));

            return new NullJsonResult();
        }
        #endregion
        #endregion

        #region Accordingly
        [HttpPost]
        public IActionResult AccordinglyList(int sliderGroupId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _anywhereSliderFactory.PrepareAccordinglyListModel(sliderGroupId);
            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult AccordinglyDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a setting with the specified id
            var image = _anywhereSliderService.GetAccordinglyById(id);

            _anywhereSliderService.DeleteAccordingly(image);

            return new NullJsonResult();
        }

        public virtual IActionResult AccordinglyAddPopup(int sliderGroupId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            Action<AccordinglyModelLocalizedModel, int> localizedModelConfiguration = null;

            var model = new AccordinglyModel
            {
                SliderGroupId = sliderGroupId,
                AvailablePositions = PreparePositions(0),
                AvailableAlignments = PrepareAlignments(0)
            };

            model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            ViewBag.RefreshPage = false;
            return View("AccordinglyAddPopup", model);
        }

        [HttpPost]
        public virtual IActionResult AccordinglyAddPopup(AccordinglyModel accordinglyModel, string btnId, string formId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var sliderGroup = _anywhereSliderService.GetsliderGroupById(accordinglyModel.SliderGroupId);
            var image = new Accordingly
            {
                SliderId = sliderGroup.SliderId,
                SliderGroupId = accordinglyModel.SliderGroupId,
                PictureId = accordinglyModel.PictureId,
                MobilePictureId = accordinglyModel.MobilePictureId,
                TabletPictureId = accordinglyModel.TabletPictureId,
                ClickToAction = accordinglyModel.ClickToAction,
                Position = accordinglyModel.Position,
                Alignment = accordinglyModel.Alignment,
                Html = accordinglyModel.Html,
                DisplayOrder = accordinglyModel.DisplayOrder
            };

            _anywhereSliderService.InsertAccordingly(image);
            UpdateLocales(image, accordinglyModel);

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View("AccordinglyAddPopup", accordinglyModel);
        }

        public virtual IActionResult AccordinglyEditPopup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            Action<AccordinglyModelLocalizedModel, int> localizedModelConfiguration = null;

            var accordingly = _anywhereSliderService.GetAccordinglyById(id);
            if (accordingly == null)
            {
                var accordinglyModel = new AccordinglyModel();
                accordinglyModel.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

                ViewBag.RefreshPage = true;
                return View("AccordinglyEditPopup", accordinglyModel);
            }

            var model = new AccordinglyModel
            {
                Id = accordingly.Id,
                SliderGroupId = accordingly.SliderGroupId,
                PictureId = accordingly.PictureId,
                MobilePictureId = accordingly.MobilePictureId,
                TabletPictureId = accordingly.TabletPictureId,
                ClickToAction = accordingly.ClickToAction,
                Position = accordingly.Position,
                Alignment = accordingly.Alignment,
                Html = accordingly.Html,
                DisplayOrder = accordingly.DisplayOrder,
                AvailablePositions = PreparePositions(accordingly.Position ?? 0),
                AvailableAlignments = PrepareAlignments(accordingly.Position ?? 0)
            };

            //define localized model configuration action
            localizedModelConfiguration = (locale, languageId) =>
            {
                locale.PictureId = _localizationService.GetLocalized(accordingly, entity => entity.PictureId, languageId, false, false);
                locale.MobilePictureId = _localizationService.GetLocalized(accordingly, entity => entity.MobilePictureId, languageId, false, false);
                locale.TabletPictureId = _localizationService.GetLocalized(accordingly, entity => entity.TabletPictureId, languageId, false, false);
                locale.Position = _localizationService.GetLocalized(accordingly, entity => entity.Position, languageId, false, false);
                locale.Alignment = _localizationService.GetLocalized(accordingly, entity => entity.Alignment, languageId, false, false);
                locale.ClickToAction = _localizationService.GetLocalized(accordingly, entity => entity.ClickToAction, languageId, false, false);
                locale.Html = _localizationService.GetLocalized(accordingly, entity => entity.Html, languageId, false, false);
                locale.AvailablePositions = PreparePositions(locale.Position ?? 0);
                locale.AvailableAlignments = PrepareAlignments(locale.Alignment ?? 0);
            };
            model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            ViewBag.RefreshPage = false;
            return View("AccordinglyEditPopup", model);
        }

        [HttpPost]
        public virtual IActionResult AccordinglyEditPopup(AccordinglyModel accordinglyModel, string btnId, string formId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var accordingly = _anywhereSliderService.GetAccordinglyById(accordinglyModel.Id);

            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var sliderGroup = _anywhereSliderService.GetsliderGroupById(accordinglyModel.SliderGroupId);
            accordingly.SliderId = sliderGroup.SliderId;
            accordingly.SliderGroupId = accordinglyModel.SliderGroupId;
            accordingly.Html = accordinglyModel.Html;
            accordingly.PictureId = accordinglyModel.PictureId > 0 ? accordinglyModel.PictureId : null;
            accordingly.MobilePictureId = accordinglyModel.MobilePictureId > 0 ? accordinglyModel.MobilePictureId : null;
            accordingly.TabletPictureId = accordinglyModel.TabletPictureId > 0 ? accordinglyModel.TabletPictureId : null;
            accordingly.ClickToAction = accordinglyModel.ClickToAction;
            accordingly.Position = accordinglyModel.Position > 0 ? accordinglyModel.Position : null;
            accordingly.Alignment = accordinglyModel.Alignment > 0 ? accordinglyModel.Alignment : null;
            accordingly.DisplayOrder = accordinglyModel.DisplayOrder;

            _anywhereSliderService.UpdateAccordingly(accordingly);
            UpdateLocales(accordingly, accordinglyModel);

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View("AccordinglyEditPopup", accordinglyModel);
        }
        #endregion

        #region Slider Group
        public IActionResult SliderGroupList(int sliderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _anywhereSliderFactory.PrepareSliderGroupListModel(sliderId);
            return Json(model);
        }

        public IActionResult CreateSliderGroup(int sliderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            Action<SliderGroupModelLocalizedModel, int> localizedModelConfiguration = null;

            var model = _anywhereSliderFactory.PrepareSliderGroupModel(null);
            model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);
            model.SliderId = sliderId;
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateSliderGroup(SliderGroupModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var sliderGroup = new SliderGroup()
                {
                    SliderId = model.SliderId,
                    Title = model.Title,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder
                };
                _anywhereSliderService.InsertSliderGroup(sliderGroup);
                UpdateSliderGroupLocales(sliderGroup, model);
                SuccessNotification(_localizationService.GetResource("Widgets.AnywhereSlider.SliderGroup.Added"));
                model = _anywhereSliderFactory.PrepareSliderGroupModel(sliderGroup);
            }
            return RedirectToAction("EditSliderGroup", new { id = model.Id });
        }

        public IActionResult EditSliderGroup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();
            Action<SliderGroupModelLocalizedModel, int> localizedModelConfiguration = null;
            var sliderGroup = _anywhereSliderService.GetsliderGroupById(id);

            var model = _anywhereSliderFactory.PrepareSliderGroupModel(sliderGroup);
            localizedModelConfiguration = (locale, languageId) =>
            {
                locale.Title = _localizationService.GetLocalized(sliderGroup, entity => entity.Title, languageId, false, false);
                locale.Description = _localizationService.GetLocalized(sliderGroup, entity => entity.Description, languageId, false, false);
            };
            model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditSliderGroup(SliderGroupModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var sliderGroup = _anywhereSliderService.GetsliderGroupById(model.Id);

            if (ModelState.IsValid)
            {
                sliderGroup.SliderId = model.SliderId;
                sliderGroup.Title = model.Title;
                sliderGroup.Description = model.Description;
                sliderGroup.DisplayOrder = model.DisplayOrder;
                _anywhereSliderService.UpdateSliderGroup(sliderGroup);
                UpdateSliderGroupLocales(sliderGroup, model);

                SuccessNotification(_localizationService.GetResource("Widgets.AnywhereSlider.SliderGroup.Updated"));
                return RedirectToAction("EditSliderGroup", new { id = model.Id });
            }

            return View(model);
        }

        public IActionResult DeleteSliderGroup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var sliderGroup = _anywhereSliderService.GetsliderGroupById(id);
            if (sliderGroup != null)
            {
                _anywhereSliderService.DeleteSliderGroup(sliderGroup);
                SuccessNotification(_localizationService.GetResource("Widgets.AnywhereSlider.SliderGroup.Deleted"));
            }
            return new NullJsonResult();
        }
        #endregion
    }
}
