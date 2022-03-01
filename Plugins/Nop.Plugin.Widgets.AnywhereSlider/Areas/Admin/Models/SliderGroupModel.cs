using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models
{
    public class SliderGroupModel : BaseNopEntityModel, ILocalizedModel<SliderGroupModelLocalizedModel>
    {
        public SliderGroupModel()
        {
            Locales = new List<SliderGroupModelLocalizedModel>();
        }
        public int SliderId { get; set; }
        [NopResourceDisplayName("Widgets.AnywhereSlider.SliderGroup.Fields.Title")]
        public string Title { get; set; }
        [NopResourceDisplayName("Widgets.AnywhereSlider.SliderGroup.Fields.Descrption")]
        public string Description { get; set; }

        [NopResourceDisplayName("Widgets.AnywhereSlider.Accordingly.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        public IList<SliderGroupModelLocalizedModel> Locales { get; set; }
    }

    public partial class SliderGroupModelLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Widgets.AnywhereSlider.SliderGroup.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Widgets.AnywhereSlider.SliderGroup.Fields.Descrption")]
        public string Description { get; set; }
    }
}
