using Nop.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models;
using Nop.Plugin.Widgets.AnywhereSlider.Domains;

namespace Nop.Plugin.Widgets.AnywhereSlider.Areas.Admin.Factories
{
    public interface IAnywhereSliderFactory
    {
        SliderListModel PrepareSliderListModel(SliderSearchModel searchModel);

        SliderModel PrepareSliderModel(Slider slider, SliderModel sliderModel);

        AccordinglyListModel PrepareAccordinglyListModel(int sliderId);
        SliderGroupListModel PrepareSliderGroupListModel(int sliderId);
        SliderGroupModel PrepareSliderGroupModel(SliderGroup sliderGroup);
    }
}
