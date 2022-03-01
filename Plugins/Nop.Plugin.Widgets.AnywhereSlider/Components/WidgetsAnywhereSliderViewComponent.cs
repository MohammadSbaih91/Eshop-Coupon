using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using System.Linq.Expressions;
using Nop.Plugin.Widgets.AnywhereSlider.Infrastructure.Cache;
using Nop.Plugin.Widgets.AnywhereSlider.Models;
using Nop.Plugin.Widgets.AnywhereSlider.Services;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Components;
using System;
using System.Linq;

namespace Nop.Plugin.Widgets.AnywhereSlider.Components
{
    [ViewComponent(Name = "WidgetsAnywhereSlider")]
    public class WidgetsAnywhereSliderViewComponent : NopViewComponent
    {
        private readonly IAnywhereSliderService _anywhereSliderService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;

        public WidgetsAnywhereSliderViewComponent(IAnywhereSliderService anywhereSliderService,
            IStaticCacheManager cacheManager,
            IPictureService pictureService,
            ILocalizationService localizationService)
        {
            _anywhereSliderService = anywhereSliderService;
            _cacheManager = cacheManager;
            _pictureService = pictureService;
            _localizationService = localizationService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var data = _anywhereSliderService.GetSliderByWidgetsPlace(widgetZone);

            if (data != null)
            {
                if (data.SliderGroups.Count <= 0)
                    return Content("");

                var model = new PublicInfoModel()
                {
                    SliderId = data.Id,
                    Name = data.Name
                };
                var sliderGroupList = data.SliderGroups.Take(4);
                foreach (var item in sliderGroupList)
                {
                    var sliderGroup = new SliderGroupInfo()
                    {
                        Id = item.Id,
                        SliderId = item.SliderId,
                        Title = _localizationService.GetLocalized(item, x => x.Title),
                        Description = _localizationService.GetLocalized(item, x => x.Description),
                        DisplayOrder = item.DisplayOrder
                    };

                    var accordinglies = _anywhereSliderService.GetAccordinglyBySliderId(sliderGroup.Id);
                    foreach (var accordingly in accordinglies)
                    {
                        var pictireId = _localizationService.GetLocalized(accordingly, x => x.PictureId);
                        var mobilePictureId = _localizationService.GetLocalized(accordingly, x => x.MobilePictureId);
                        var tabletPictireId = _localizationService.GetLocalized(accordingly, x => x.TabletPictureId);
                        var position = _localizationService.GetLocalized(accordingly, x => x.Position);
                        var alignment = _localizationService.GetLocalized(accordingly, x => x.Alignment);

                        var acc = new AccordinglyInfo()
                        {
                            SliderId = accordingly.SliderId,
                            Html = _localizationService.GetLocalized(accordingly, x => x.Html),
                            PictureId = pictireId ?? 0,
                            MobilePictureId = mobilePictureId ?? 0,
                            TabletPictureId = tabletPictireId ?? 0,
                            Position = position ?? 0,
                            Alignment = alignment ?? 0,
                            ClickToAction = _localizationService.GetLocalized(accordingly, x => x.ClickToAction),
                            DisplayOrder = accordingly.DisplayOrder
                        };
                        acc.PictureUrl = GetPictureUrl(acc.PictureId);
                        acc.MobilePictureUrl = GetPictureUrl(acc.MobilePictureId);
                        acc.TabletPictureUrl = GetPictureUrl(acc.TabletPictureId);
                        sliderGroup.AccordinglyInfos.Add(acc);
                    }
                    if (accordinglies.Any())
                        model.SliderGroupInfos.Add(sliderGroup);
                }

                return View("~/Plugins/Widgets.AnywhereSlider/Views/PublicInfo.cshtml", model);
            }
            return Content("");
        }


        protected string GetPictureUrl(int pictureId)
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.PICTURE_URL_MODEL_KEY, pictureId);
            return _cacheManager.Get(cacheKey, () =>
            {
                //little hack here. nulls aren't cacheable so set it to ""
                var url = _pictureService.GetPictureUrl(pictureId, showDefaultPicture: false) ?? "";
                return url;
            });
        }
    }
}
