using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models;
using Nop.Plugin.Widgets.AnywhereSlider.Domains;
using Nop.Plugin.Widgets.AnywhereSlider.Services;
using Nop.Services.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Widgets.AnywhereSlider.Areas.Admin.Factories
{
    public class AnywhereSliderFactory : IAnywhereSliderFactory
    {
        #region Fields
        private readonly IAnywhereSliderService _anywhereSliderService;
        private readonly IPictureService _pictureService;
        #endregion

        #region Ctor
        public AnywhereSliderFactory(IAnywhereSliderService anywhereSliderService,
            IPictureService pictureService)
        {
            _anywhereSliderService = anywhereSliderService;
            _pictureService = pictureService;
        }
        #endregion

        #region Utilities
        private IList<SelectListItem> PrepareWidgetsList(string widgetZone)
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Text = "home_page_top",
                Value = "home_page_top",
                Selected = "home_page_top" == widgetZone
            });
            list.Add(new SelectListItem()
            {
                Text = "home_page_bottom",
                Value = "home_page_bottom",
                Selected = "home_page_bottom" == widgetZone
            });
            list.Add(new SelectListItem()
            {
                Text = "categorydetails_bottom",
                Value = "categorydetails_bottom",
                Selected = "categorydetails_bottom" == widgetZone
            });
            list.Add(new SelectListItem()
            {
                Text = "categorydetails_top",
                Value = "categorydetails_top",
                Selected = "categorydetails_top" == widgetZone
            });
            list.Add(new SelectListItem() { Text = "categorydetails_top_MobilePlans", Value = "categorydetails_top_MobilePlans", Selected = "categorydetails_top_MobilePlans" == widgetZone });
            list.Add(new SelectListItem() { Text = "categorydetails_top_Internet", Value = "categorydetails_top_Internet", Selected = "categorydetails_top_Internet" == widgetZone });
            list.Add(new SelectListItem() { Text = "categorydetails_top_SmartLife", Value = "categorydetails_top_SmartLife", Selected = "categorydetails_top_SmartLife" == widgetZone });
            list.Add(new SelectListItem() { Text = "categorydetails_top_FixedLine", Value = "categorydetails_top_FixedLine", Selected = "categorydetails_top_FixedLine" == widgetZone });

            return list;
        }
        #endregion

        #region Methods

        public SliderListModel PrepareSliderListModel(SliderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var sliders = _anywhereSliderService.GetAllSlider(
                    name: searchModel.SearchName,
                    widgetsPlace: "",
                    pageIndex: searchModel.Page - 1,
                    pageSize: searchModel.PageSize);

            //prepare list model
            var model = new SliderListModel
            {
                Data = sliders.Select(slider =>
                {
                    return new SliderModel()
                    {
                        Id = slider.Id,
                        Name = slider.Name,
                        WidgetZone = slider.WidgetZone,
                        Published = slider.Published
                    };
                }),
                Total = sliders.TotalCount
            };

            return model;
        }

        public SliderModel PrepareSliderModel(Slider slider, SliderModel sliderModel)
        {
            if (slider == null)
            {
                var sdModel = new SliderModel()
                {
                    WidgetZoneList = PrepareWidgetsList("")
                };
                if (sliderModel != null)
                {
                    sdModel.Name = sliderModel.Name;
                    sdModel.WidgetZone = sliderModel.WidgetZone;
                    sdModel.WidgetZoneList = PrepareWidgetsList(slider.WidgetZone);
                    sdModel.Published = sliderModel.Published;
                }
                return sdModel;
            }
            else
            {
                var sdModel = new SliderModel()
                {
                    Id = slider.Id,
                    Name = slider.Name,
                    WidgetZone = slider.WidgetZone,
                    WidgetZoneList = PrepareWidgetsList(slider.WidgetZone),
                    Published = slider.Published,
                };

                return sdModel;
            }
        }

        public AccordinglyListModel PrepareAccordinglyListModel(int sliderGroupId)
        {
            if (sliderGroupId == 0)
                return new AccordinglyListModel();

            var accordingly = _anywhereSliderService.GetAccordinglyBySliderId(sliderGroupId);


            //prepare list model
            var model = new AccordinglyListModel
            {
                Data = accordingly.Select(item =>
                {
                    var picture = _pictureService.GetPictureById(item.PictureId ?? 0);
                    return new AccordinglyModel()
                    {
                        Id = item.Id,
                        SliderId = item.SliderId,
                        Html = item.Html,
                        PictureId = item.PictureId,
                        TabletPictureId = item.TabletPictureId,
                        MobilePictureId = item.MobilePictureId,
                        Position = item.Position,
                        strAlignment = item.Alignment != null ? Enum.GetName(typeof(AlignmentEnum), item.Alignment) : "",
                        strPosition = item.Position != null ? Enum.GetName(typeof(PositionEnum), item.Position): "",
                        Alignment = item.Alignment,
                        DisplayOrder = item.DisplayOrder,
                        PictureUrl = _pictureService.GetPictureUrl(picture)
                };
                }),
                Total = accordingly.Count()
            };

            return model;
        }

        public SliderGroupListModel PrepareSliderGroupListModel(int sliderId)
        {
            if (sliderId == 0)
                return new SliderGroupListModel();

            var sliderGroup = _anywhereSliderService.GetSliderGroupBySliderId(sliderId);


            //prepare list model
            var model = new SliderGroupListModel
            {
                Data = sliderGroup.Select(item =>
                {
                    return new SliderGroupModel()
                    {
                        Id = item.Id,
                        SliderId = item.SliderId,
                        Title = item.Title,
                        Description = item.Description,
                        DisplayOrder = item.DisplayOrder,
                    };
                }),
                Total = sliderGroup.Count()
            };

            return model;
        }

        public SliderGroupModel PrepareSliderGroupModel(SliderGroup sliderGroup)
        {
            var sdModel = new SliderGroupModel();
            if (sliderGroup != null)
            {
                sdModel = new SliderGroupModel()
                {
                    Id = sliderGroup.Id,
                    SliderId = sliderGroup.SliderId,
                    Title = sliderGroup.Title,
                    Description = sliderGroup.Description,
                    DisplayOrder = sliderGroup.DisplayOrder
                };
            }
            return sdModel;
        }
        #endregion
    }
}
