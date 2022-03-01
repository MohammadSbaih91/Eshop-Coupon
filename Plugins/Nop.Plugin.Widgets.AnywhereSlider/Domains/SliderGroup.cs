using Nop.Core;
using Nop.Core.Domain.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Widgets.AnywhereSlider.Domains
{
    public partial class SliderGroup : BaseEntity, ILocalizedEntity
    {
        private ICollection<Accordingly> _Accordinglies;
        public int SliderId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool? Deleted { get; set; }

        public virtual ICollection<Accordingly> Accordinglies
        {
            get => _Accordinglies ?? (_Accordinglies = new List<Accordingly>());
            protected set => _Accordinglies = value;
        }
    }
}
