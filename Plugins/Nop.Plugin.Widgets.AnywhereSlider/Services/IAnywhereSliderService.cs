using Nop.Core;
using Nop.Plugin.Widgets.AnywhereSlider.Domains;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.AnywhereSlider.Services
{
    public interface IAnywhereSliderService
    {
        #region Slider

        IPagedList<Slider> GetAllSlider(string name, string widgetsPlace, int pageIndex = 0, int pageSize = int.MaxValue);

        Slider GetSliderById(int id);

        Slider GetSliderByWidgetsPlace(string WidgetsPlace);

        void InsertSlider(Slider slider);

        void UpdateSlider(Slider slider);

        void DeleteSlider(Slider slider);
        #endregion

        #region Accordingly
        IList<Accordingly> GetAccordinglyBySliderId(int sliderGroupId);

        Accordingly GetAccordinglyById(int id);

        void InsertAccordingly(Accordingly accordingly);

        void UpdateAccordingly(Accordingly accordingly);

        void DeleteAccordingly(Accordingly accordingly);
        #endregion

        #region Slider Group
        void InsertSliderGroup(SliderGroup sliderGroup);
        void UpdateSliderGroup(SliderGroup sliderGroup);
        void DeleteSliderGroup(SliderGroup sliderGroup);
        SliderGroup GetsliderGroupById(int id);
        IList<SliderGroup> GetSliderGroupBySliderId(int sliderId);
        #endregion
    }
}
