using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models
{
    public class AccordinglyModel : BaseNopEntityModel, ILocalizedModel<AccordinglyModelLocalizedModel>
    {
        public int SliderId { get; set; }
        public int SliderGroupId { get; set; }
        [UIHint("Picture")]
        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.Picture")]
        public int? PictureId { get; set; }
        [UIHint("Picture")]
        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.MobilePictureId")]
        public int? MobilePictureId { get; set; }
        [UIHint("Picture")]
        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.TabletPictureId")]
        public int? TabletPictureId { get; set; }
        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.Position")]
        public int? Position { get; set; }
        public string strPosition { get; set; }
        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.Alignment")]
        public int? Alignment { get; set; }
        public string strAlignment { get; set; }
        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.ClickToAction")]
        public string ClickToAction { get; set; }

        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.Html")]
        public string Html { get; set; }
        public string PictureUrl { get; set; }

        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.DisplayOrder")]
        public string DisplayOrder { get; set; }

        public IList<SelectListItem> AvailablePositions { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem> AvailableAlignments { get; set; } = new List<SelectListItem>();

        public IList<AccordinglyModelLocalizedModel> Locales { get; set; }
    }

    public partial class AccordinglyModelLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.Picture")]
        public int? PictureId { get; set; }
        [UIHint("Picture")]
        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.MobilePictureId")]
        public int? MobilePictureId { get; set; }
        [UIHint("Picture")]
        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.TabletPictureId")]
        public int? TabletPictureId { get; set; }
        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.Position")]
        public int? Position { get; set; }
        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.Alignment")]
        public int? Alignment { get; set; }
        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.ClickToAction")]
        public string ClickToAction { get; set; }
        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.Html")]
        public string Html { get; set; }

        public IList<SelectListItem> AvailablePositions { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem> AvailableAlignments { get; set; } = new List<SelectListItem>();
    }
}
