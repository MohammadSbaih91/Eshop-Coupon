using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.AnywhereSlider.Models
{
    public class PublicInfoModel : BaseNopModel
    {
        public PublicInfoModel()
        {
            SliderGroupInfos = new List<SliderGroupInfo>();
        }

        public int SliderId { get; set; }
        public string Name { get; set; }
        public IList<SliderGroupInfo> SliderGroupInfos { get; set; }
    }

    public class SliderGroupInfo : BaseNopModel
    {
        public SliderGroupInfo()
        {
            AccordinglyInfos = new List<AccordinglyInfo>();
        }
        public int Id { get; set; }
        public int SliderId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public IList<AccordinglyInfo> AccordinglyInfos { get; set; }
    }

    public class AccordinglyInfo : BaseNopModel
    {
        public int SliderId { get; set; }

        public string Html { get; set; }
        public int PictureId { get; set; }
        public int MobilePictureId { get; set; }
        public int TabletPictureId { get; set; }
        public int Position { get; set; }
        public int Alignment { get; set; }
        public string ClickToAction { get; set; }
        public string DisplayOrder { get; set; }

        public string PictureUrl { get; set; }
        public string MobilePictureUrl { get; set; }
        public string TabletPictureUrl { get; set; }
        

    }
}
