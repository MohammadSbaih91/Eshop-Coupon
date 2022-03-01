using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models
{
    public class SliderModel : BaseNopEntityModel
    {
        public SliderModel()
        {
            WidgetZoneList = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Widgets.AnywhereSlider.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Widgets.AnywhereSlider.Fields.WidgetZone")]
        public string WidgetZone { get; set; }
        public IList<SelectListItem> WidgetZoneList { get; set; }
        
        [NopResourceDisplayName("Widgets.AnywhereSlider.Fields.Published")]
        public bool Published { get; set; }
    }
}
