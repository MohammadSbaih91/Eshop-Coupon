using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Widgets.AnywhereSlider.Domains
{
    public partial class Accordingly : BaseEntity, ILocalizedEntity
    {
        public int SliderId { get; set; }
        public int? PictureId { get; set; }
        public int? MobilePictureId { get; set; }
        public int? TabletPictureId { get; set; }
        public int? Position { get; set; }
        public int? Alignment { get; set; }
        public string ClickToAction { get; set; }
        public string Html { get; set; }

        public string DisplayOrder { get; set; }
        public int SliderGroupId { get; set; }
    }
}
