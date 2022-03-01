using Nop.Core;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.AnywhereSlider.Domains
{
    public partial class Slider : BaseEntity
    {
        private ICollection<SliderGroup> _SliderGroups;
        
        public string Name { get; set; }

        public string WidgetZone { get; set; }
        
        public bool Published { get; set; }

        public bool Deleted { get; set; }

        public virtual ICollection<SliderGroup> SliderGroups
        {
            get => _SliderGroups ?? (_SliderGroups = new List<SliderGroup>());
            protected set => _SliderGroups = value;
        }
    }
}
